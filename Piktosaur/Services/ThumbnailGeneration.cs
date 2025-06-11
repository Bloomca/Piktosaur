using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Piktosaur.Utils;
using Windows.Devices.Radios;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Piktosaur.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class ThumbnailGeneration
    {
        public static ThumbnailGeneration Shared = new ThumbnailGeneration();

        private readonly SemaphoreSlim osThumbnailSemaphore;
        private readonly SemaphoreSlim fallbackThumbnailSemaphore;

        private readonly SmartQueue smartQueue;

        private readonly Dictionary<string, Boolean> thumbnailsGenerating = [];

        public ThumbnailGeneration()
        {
            osThumbnailSemaphore = new SemaphoreSlim(1, 1);
            fallbackThumbnailSemaphore = new SemaphoreSlim(1, 1);

            smartQueue = new SmartQueue(this);
        }

        public async Task<BitmapSource?> GenerateThumbnail(string path, CancellationToken cancellationToken)
        {
            if (thumbnailsGenerating.ContainsKey(path)) return null;

            try
            {
                if (thumbnailsGenerating.ContainsKey(path)) return null;
                thumbnailsGenerating.TryAdd(path, true);

                return await smartQueue.AddRequest(path, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {

                // Log the full exception details including call stack
                System.Diagnostics.Debug.WriteLine($"Thumbnail generation failed:");
                System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // If it's a COM exception, get the HRESULT
                if (ex is System.Runtime.InteropServices.COMException comEx)
                {
                    System.Diagnostics.Debug.WriteLine($"HRESULT: 0x{comEx.HResult:X8}");
                }

                Debug.WriteLine($"Error during generating thumbnail: {ex}");
                return null;
            }
            finally
            {
                thumbnailsGenerating.Remove(path, out bool _result);
            }
        }

        public async Task<BitmapSource> CreateManualThumbnail(string path, CancellationToken cancellationToken)
        {
            await osThumbnailSemaphore.WaitAsync(cancellationToken);

            try
            {
                var (thumbnailData, ratio) = await GeneratePixelData(path, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                await WaitForFrameOpportunity();

                cancellationToken.ThrowIfCancellationRequested();

                var writeableBitmap = new WriteableBitmap(200, (int)(200 / ratio));
                using (var pixelStream = writeableBitmap.PixelBuffer.AsStream())
                {
                    await pixelStream.WriteAsync(thumbnailData, 0, thumbnailData.Length, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();

                return writeableBitmap;
            }
            finally
            {
                osThumbnailSemaphore.Release();
            }
        }

        private async Task WaitForFrameOpportunity()
        {
            // wait for one frame
            await Task.Delay(16);
            var tcs = new TaskCompletionSource<bool>();

            void OnRendering(object sender, object e)
            {
                CompositionTarget.Rendering -= OnRendering;
                tcs.SetResult(true);
            }

            CompositionTarget.Rendering += OnRendering;
            await tcs.Task;
        }

        private async Task<(byte[], double)> GeneratePixelData(string path, CancellationToken cancellationToken)
        {
            //await fallbackThumbnailSemaphore.WaitAsync(cancellationToken);

            try
            {
                return await Task.Run(async () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using var fileStream = System.IO.File.OpenRead(path);
                    using var randomAccessStream = fileStream.AsRandomAccessStream();

                    cancellationToken.ThrowIfCancellationRequested();

                    var decoder = await BitmapDecoder.CreateAsync(randomAccessStream)
                        .AsTask(cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    double ratio = (double)decoder.PixelWidth / decoder.PixelHeight;

                    var transform = new BitmapTransform
                    {
                        ScaledWidth = 200,
                        ScaledHeight = (uint)(200 / ratio),
                        InterpolationMode = BitmapInterpolationMode.Fant
                    };

                    cancellationToken.ThrowIfCancellationRequested();

                    var pixelData = await decoder.GetPixelDataAsync(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Premultiplied,
                        transform,
                        ExifOrientationMode.RespectExifOrientation,
                        ColorManagementMode.DoNotColorManage
                    ).AsTask(cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    return (pixelData.DetachPixelData(), ratio);
                }, cancellationToken);
            }
            finally
            {
                //fallbackThumbnailSemaphore.Release();
            }
        }
    }

    public class SmartQueue
    {
        private readonly int MAX_REQUESTS = 15;
        private List<QueueItem> requests = new();

        private ThumbnailGeneration thumbnailGeneration;

        private bool isExecuting = false;

        public SmartQueue(ThumbnailGeneration thumbnailGeneration)
        {
            this.thumbnailGeneration = thumbnailGeneration;
        }

        public Task<BitmapSource?> AddRequest(string path, CancellationToken ct)
        {
            TaskCompletionSource<BitmapSource?> tcs = new();
            if (requests.Count > MAX_REQUESTS)
            {
                var oldestRequest = requests.Last();
                oldestRequest.Tcs.SetResult(null);
                requests.Remove(oldestRequest);
            }

            requests.Insert(0, new QueueItem(tcs, path, ct));
            ExecuteRequests();

            return tcs.Task;
        }

        public async void ExecuteRequests()
        {
            if (isExecuting) return;

            isExecuting = true;

            while (GetNextRequest() is var newRequest && newRequest != null)
            {
                // remove the request first from the data structure
                requests.Remove(newRequest);
                
                try
                {
                    var result = await thumbnailGeneration.CreateManualThumbnail(newRequest.Path, newRequest.ct);
                    newRequest.Tcs.SetResult(result);
                }
                catch
                {
                    newRequest.Tcs.SetResult(null);
                }
            }

            isExecuting = false;
        }

        private QueueItem? GetNextRequest()
        {
            if (requests.Count == 0) return null;
            return requests.Last();
        }

        public void Clear()
        {
            foreach (var request in requests)
            {
                request.Tcs.SetResult(null);
            }

            requests.Clear();
        }
    }

    public class QueueItem
    {
        public TaskCompletionSource<BitmapSource?> Tcs;
        public string Path;
        public CancellationToken ct;

        public QueueItem(TaskCompletionSource<BitmapSource?> tcs, string path, CancellationToken ct)
        {
            Tcs = tcs;
            Path = path;
            this.ct = ct;
        }
    }
}
