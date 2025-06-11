using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;

namespace Piktosaur.Services
{
    public class ManualThumbnailGenerator
    {
        /// <summary>
        /// Create a thumbnail for the images passed in path. It is called "manual" because
        /// there is WinRT API `StorageFile.GetThumbnailAsync`: https://learn.microsoft.com/en-us/uwp/api/windows.storage.storagefile.getthumbnailasync?view=winrt-26100
        /// 
        /// That API proved to be quite unreliable and has questionable performance, so instead
        /// this class provides static method to read the file and apply bitmap transformation manually.
        /// 
        /// This is not a silver bullet, as it is possible to run into artifacts, and it seems to be
        /// constrained by bandwidth/memory/GPU resources, as parallelizing does not help much.
        /// </summary>
        public static async Task<BitmapSource> CreateManualThumbnail(string path, CancellationToken cancellationToken)
        {
            var (thumbnailData, ratio) = await GeneratePixelData(path, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await WaitForFrameOpportunity();

            cancellationToken.ThrowIfCancellationRequested();

            // we create bitmap image in the UI thread, because WinUI applications
            // do not allow to offload that to other threads

            var writeableBitmap = new WriteableBitmap(200, (int)(200 / ratio));
            using (var pixelStream = writeableBitmap.PixelBuffer.AsStream())
            {
                await pixelStream.WriteAsync(thumbnailData, 0, thumbnailData.Length, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            return writeableBitmap;
        }

        /// <summary>
        /// Wait for one frame, and then subscribe to the underlying framework
        /// rendering event loop, and wait exactly 1 frame.
        /// 
        /// The idea is to ensure we resume work exactly as the UI is done with
        /// work, so we should avoid jittery experience while scrolling.
        /// </summary>
        private static async Task WaitForFrameOpportunity()
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

        /// <summary>
        /// While it is not allowed to create bitmap images in non-UI thread, it is possible
        /// to read file, perform some operations and then pass it back in binary format.
        /// 
        /// We do so in case the file is huge and reading + applying the bitmap transform
        /// takes more than 1 frame (16ms).
        /// </summary>
        private static Task<(byte[], double)> GeneratePixelData(string path, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
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
    }
}
