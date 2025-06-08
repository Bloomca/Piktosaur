using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Piktosaur.Models
{
    public class ImageResult
    {
        public string Path { get; }

        private uint? width;
        private uint? height;

        public uint? Width => width;
        public uint? Height => height;

        private bool isGenerating = false;
        public BitmapImage? Thumbnail { get; private set; }

        public ImageResult(string path)
        {
            Path = path;
            // very slow, will need to be offloaded to another thread
            // CalculateDimensions(path);
        }

        private async void CalculateDimensions(string path)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(Path);
                var properties = await file.Properties.GetImagePropertiesAsync();
                width = properties.Width;
                height = properties.Height;
            } catch
            {
                // pass
            }   
        }

        public async Task GenerateThumbnail()
        {
            if (Thumbnail != null || isGenerating) return;

            // we never put to `false`, in case there was an error
            isGenerating = true;
            StorageFile file = await StorageFile.GetFileFromPathAsync(Path);

            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(
                ThumbnailMode.SingleItem,
                200,
                ThumbnailOptions.UseCurrentScale
            );

            if (thumbnail != null)
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(thumbnail);
                thumbnail.Dispose(); // Important: dispose the stream
                Thumbnail = bitmapImage;
            }
        }
    }
}
