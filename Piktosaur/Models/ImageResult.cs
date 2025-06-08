using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

using Piktosaur.Services;

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
        public BitmapSource? Thumbnail { get; private set; }

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
            if (Thumbnail != null) return;
            var thumbnail = await ThumbnailGeneration.Shared.GenerateThumbnail(Path);
            Thumbnail = thumbnail;
        }
    }
}
