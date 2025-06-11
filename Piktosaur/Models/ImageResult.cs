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
using System.Threading;

namespace Piktosaur.Models
{
    public class ImageResult : IDisposable
    {
        public string Path { get; }

        private bool isDisposed = false;
        public BitmapSource? Thumbnail { get; private set; }

        public ImageResult(string path)
        {
            Path = path;
        }

        public async Task<Boolean> GenerateThumbnail(CancellationToken cancellationToken)
        {
            if (Thumbnail != null || isDisposed || cancellationToken.IsCancellationRequested) return false;
            var thumbnail = await ThumbnailGeneration.Shared.GenerateThumbnail(Path, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (isDisposed) return false;
            Thumbnail = thumbnail;

            return true;
        }

        public void Dispose()
        {
            if (isDisposed) return;

            isDisposed = true;
            Thumbnail = null;
        }
    }
}
