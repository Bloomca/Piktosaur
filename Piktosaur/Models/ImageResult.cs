using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Piktosaur.Models
{
    public class ImageResult
    {
        public string Path { get; }

        public BitmapImage? Thumbnail { get; private set; }

        public ImageResult(string path)
        {
            Path = path;
        }

        public async Task GenerateThumbnail()
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(Path);

            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(
                ThumbnailMode.PicturesView,
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
