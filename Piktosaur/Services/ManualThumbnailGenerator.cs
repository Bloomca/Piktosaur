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
        /// This is not a silver bullet, as it is possible to run into artifacts.
        /// </summary>
        public static async Task<ImageSource> CreateManualThumbnail(string path, CancellationToken cancellationToken)
        {
            var softwareBitmap = await Task.Run(async () => await GenerateSoftwareBitmap(path, cancellationToken));

            cancellationToken.ThrowIfCancellationRequested();

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(softwareBitmap);

            return source;
        }

        private static async Task<SoftwareBitmap> GenerateSoftwareBitmap(string path, CancellationToken cancellationToken)
        {
            using var fileStream = File.OpenRead(path);
            using var randomAccessStream = fileStream.AsRandomAccessStream();

            var decoder = await BitmapDecoder.CreateAsync(randomAccessStream).AsTask(cancellationToken);

            double ratio = (double)decoder.PixelWidth / decoder.PixelHeight;

            var transform = new BitmapTransform
            {
                ScaledWidth = 200,
                ScaledHeight = (uint)(200 / ratio),
                InterpolationMode = BitmapInterpolationMode.Fant
            };

            var softwareBitmap = await decoder.GetSoftwareBitmapAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                transform,
                ExifOrientationMode.RespectExifOrientation,
                ColorManagementMode.DoNotColorManage
            ).AsTask(cancellationToken);

            return softwareBitmap;
        }
    }
}
