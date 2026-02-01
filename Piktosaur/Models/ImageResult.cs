using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Piktosaur.Services;
using Piktosaur.ViewModels;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Piktosaur.Models
{
    public class ImageResult : BaseViewModel, IDisposable
    {
        public string Path { get; }

        private bool isDisposed = false;

        private ImageSource? _thumbnail;

        public ImageSource? Thumbnail
        {
            get => _thumbnail;
            private set => SetProperty(ref _thumbnail, value);
        }

        private IThumbnailGenerator thumbnailGenerator;

        public ImageResult(string path, IThumbnailGenerator thumbnailGenerator)
        {
            Path = path;
            this.thumbnailGenerator = thumbnailGenerator;
        }

        public async Task<Boolean> GenerateThumbnail(CancellationToken cancellationToken)
        {
            if (Thumbnail != null || isDisposed || cancellationToken.IsCancellationRequested) return false;
            var thumbnail = await thumbnailGenerator.GenerateThumbnail(Path, cancellationToken);
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
