using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Piktosaur.Models;
using Piktosaur.Services;
using static System.Net.Mime.MediaTypeNames;

namespace Piktosaur.ViewModels
{
    public class ImagesListVM : BaseViewModel, IDisposable
    {
        private AppStateVM appStateVM;

        private ThumbnailGeneration thumbnailGeneration;

        private CancellationTokenSource? cancellationTokenSource;

        private ObservableCollection<FolderWithImages> _folders = new();

        private bool hasSelectedImage = false;

        private bool loading = false;

        public bool Loading
        {
            get => loading;
            private set => SetProperty(ref loading, value);
        }

        public ImagesListVM(AppStateVM appStateVM)
        {
            this.appStateVM = appStateVM;
            thumbnailGeneration = new ThumbnailGeneration();
        }

        public async Task LoadImages(ObservableCollection<FolderWithImages> folders)
        {
            _folders = folders;
            Loading = true;

            var currentQuery = appStateVM.SelectedQuery;
            new Search(thumbnailGeneration, folders).GetImages(currentQuery.Folders[0]);

            await LoadThumbnails(folders);

            Loading = false;
        }

        private async Task LoadThumbnails(ObservableCollection<FolderWithImages> folders)
        {
            if (folders.Count == 0) return;

            var folder = folders.First();

            if (folder == null) return;

            List<Task> thumbnailTasks = [];

            cancellationTokenSource = new CancellationTokenSource();

            foreach (var image in folder.Images.Take(10))
            {
                thumbnailTasks.Add(image.GenerateThumbnail(cancellationTokenSource.Token));
            }

            var firstImage = folder.Images.First();
            if (firstImage != null)
            {
                SelectFirstImage(firstImage);
            }

            cancellationTokenSource.Token.ThrowIfCancellationRequested();

            await Task.WhenAll(thumbnailTasks);
        }

        private void SelectFirstImage(ImageResult image)
        {
            if (hasSelectedImage == true) return;

            appStateVM.SelectImage(image.Path);
            hasSelectedImage = true;
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            foreach (var folder in _folders)
            {
                folder?.Dispose();
            }

            thumbnailGeneration.Dispose();
        }
    }
}
