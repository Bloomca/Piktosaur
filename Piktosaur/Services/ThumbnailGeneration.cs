using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Piktosaur.Utils;
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

        private readonly Dictionary<string, Boolean> thumbnailsGenerating = [];

        public ThumbnailGeneration()
        {
            var semaphoreCount = Environment.ProcessorCount >= 6 ? 3 : 2;
            osThumbnailSemaphore = new SemaphoreSlim(semaphoreCount, semaphoreCount);
            fallbackThumbnailSemaphore = new SemaphoreSlim(semaphoreCount, semaphoreCount);
        }

        public async Task<BitmapSource?> GenerateThumbnail(string path)
        {
            if (thumbnailsGenerating.ContainsKey(path)) return null;

            await osThumbnailSemaphore.WaitAsync();

            try
            {
                if (thumbnailsGenerating.ContainsKey(path)) return null;
                thumbnailsGenerating.TryAdd(path, true);

                StorageItemThumbnail? thumbnail = await GenerateOSThumbnailWithRetries(path);

                thumbnailsGenerating.Remove(path, out bool _result);

                if (thumbnail == null)
                {
                    Debug.WriteLine("Could not generate the bitmap image");
                    return null;
                }
                else
                {
                    Debug.WriteLine("Generating bitmap image");
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(thumbnail);
                    thumbnail.Dispose();

                    Debug.WriteLine("Generated bitmap image, releasing semaphore");
                                        return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during generating thumbnail: {ex}");
                return null;
            }
            finally
            {
                osThumbnailSemaphore.Release();
            }
        }

        private Task<StorageItemThumbnail?> GenerateOSThumbnailWithRetries(string path)
        {
            return GenerateOSThumbnailWithRetries(path, 1, 3);
        }

        private async Task<StorageItemThumbnail?> GenerateOSThumbnailWithRetries(string path, int currentRetry, int maxRetries)
        {
            try
            {
                var result = await GenerateOSThumbnail(path);

                if (result != null)
                {
                    return result;
                }
            }
            catch (OperationCanceledException) {
                Debug.WriteLine("Timed out generating a thumbnail using OS");
                return null;
            }

            if (currentRetry >= maxRetries) {
                return null;
            }

            // to give the thumbnail service a chance to recover
            await Task.Delay(500);

            return await GenerateOSThumbnailWithRetries(path, currentRetry + 1, maxRetries);
        }

        private async Task<StorageItemThumbnail?> GenerateOSThumbnail(string path)
        {
            try
            {
                await Throttler.ThrottleWinRT();
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);

                Debug.WriteLine("Getting OS Thumbnail");
                var thumbnailTask = file.GetThumbnailAsync(
                    ThumbnailMode.SingleItem,
                    200,
                    ThumbnailOptions.UseCurrentScale
                ).AsTask();
                var timeoutTask = Task.Delay(2500);

                var completed = await Task.WhenAny(thumbnailTask, timeoutTask);

                if (completed == timeoutTask)
                {
                    Debug.WriteLine("Thumbnail generation timed out");
                    throw new OperationCanceledException("Thumbnail generation timed out");
                }

                var thumbnail = await thumbnailTask;

                if (thumbnail != null)
                {
                    if (thumbnail.Type == ThumbnailType.Icon)
                    {
                        // we are only interested in images;
                        thumbnail.Dispose();
                        System.Diagnostics.Debug.WriteLine("Generated an icon instead of image");
                        return null;
                    }
                    return thumbnail;
                }
            }
            catch (OperationCanceledException)
            {
                throw; // Re-throw to be caught by caller
            }
            catch (COMException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Thumbnail failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error: {ex.Message}");
            }

            Debug.WriteLine("Could not get OS Thumbnail");
            return null;
        }

        private async Task<BitmapSource> CreateManualThumbnail(string path)
        {
            await fallbackThumbnailSemaphore.WaitAsync();
            
            var (thumbnailData, ratio) = await Task.Run(async () =>
            {
                var file = await StorageFile.GetFileFromPathAsync(path);
                var properties = await file.Properties.GetImagePropertiesAsync();
                double ratio = (double)properties.Width / properties.Height;

                using var stream = await file.OpenReadAsync();

                var decoder = await BitmapDecoder.CreateAsync(stream);

                var transform = new BitmapTransform
                {
                    ScaledWidth = 200,
                    ScaledHeight = (uint)(200 / ratio),
                    InterpolationMode = BitmapInterpolationMode.Fant
                };

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    transform,
                    ExifOrientationMode.RespectExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                return (pixelData.DetachPixelData(), ratio);
            });

            var writeableBitmap = new WriteableBitmap(200, (int)(200 / ratio));
            using (var pixelStream = writeableBitmap.PixelBuffer.AsStream())
            {
                await pixelStream.WriteAsync(thumbnailData, 0, thumbnailData.Length);
            }

            fallbackThumbnailSemaphore.Release();

            return writeableBitmap;
        }
    }
}
