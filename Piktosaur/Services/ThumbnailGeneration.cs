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
    public class ThumbnailGeneration : IDisposable
    {
        private bool isDisposed = false;

        private readonly SemaphoreSlim osThumbnailSemaphore;

        private readonly SmartQueue smartQueue;

        private readonly Dictionary<string, Boolean> thumbnailsGenerating = [];

        public ThumbnailGeneration()
        {
            // the original idea was to enable multiple threads, but from my testing
            // single threading model has the best UI performance
            osThumbnailSemaphore = new SemaphoreSlim(1, 1);

            smartQueue = new SmartQueue(this);
        }

        public async Task<BitmapSource?> GenerateThumbnail(string path, CancellationToken cancellationToken)
        {
            if (thumbnailsGenerating.ContainsKey(path)) return null;

            try
            {
                if (thumbnailsGenerating.ContainsKey(path)) return null;
                thumbnailsGenerating.TryAdd(path, true);

                var result = await smartQueue.AddRequest(path, cancellationToken);
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
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
                return await ManualThumbnailGenerator.CreateManualThumbnail(path, cancellationToken);
            }
            finally
            {
                osThumbnailSemaphore.Release();
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;

            isDisposed = true;

            smartQueue.Dispose();
        }
    }
}
