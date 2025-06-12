using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Piktosaur.Models;
using Piktosaur.Services;

namespace Piktosaur.ViewModels
{
    public class ImagesListVM : BaseViewModel, IDisposable
    {
        private AppStateVM appStateVM;

        private ThumbnailGeneration thumbnailGeneration;

        private List<FolderWithImages> folders = new();

        private CancellationTokenSource? cancellationTokenSource;

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

        public async Task<List<FolderWithImages>> LoadImages()
        {
            var currentQuery = appStateVM.SelectedQuery;
            var result = new Search(thumbnailGeneration).GetImages(currentQuery.Folders[0]);
            var images = result.Results;

            List<Task> thumbnailTasks = [];

            cancellationTokenSource = new CancellationTokenSource();

            foreach (var image in images.Take(10))
            {
                thumbnailTasks.Add(image.GenerateThumbnail(cancellationTokenSource.Token));
            }

            SelectFirstImage(images);

            Loading = true;

            await Task.WhenAll(thumbnailTasks);

            cancellationTokenSource.Token.ThrowIfCancellationRequested();

            Loading = false;

            var folderName = System.IO.Path.GetFileName(result.DirectoryPath);
            var folderWithImages = new FolderWithImages(folderName, images);

            folders.Add(folderWithImages);

            HandleSubdirectories(result.SubdirectoriesImagesData, folders);

            return folders;
        }

        private void HandleSubdirectories(List<ImagesData> imagesDataList, List<FolderWithImages> folders)
        {
            foreach (var imagesData in imagesDataList)
            {
                var folderName = System.IO.Path.GetFileName(imagesData.DirectoryPath);
                var folderWithImages = new FolderWithImages(folderName, imagesData.Results);

                SelectFirstImage(imagesData.Results);

                folders.Add(folderWithImages);

                HandleSubdirectories(imagesData.SubdirectoriesImagesData, folders);
            }
        }

        private void SelectFirstImage(List<ImageResult> images)
        {
            if (hasSelectedImage == true) return;

            if (images.Count > 0)
            {
                appStateVM.SelectImage(images[0].Path);
                hasSelectedImage = true;
            }
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            foreach (var folder in folders)
            {
                folder?.Dispose();
            }

            thumbnailGeneration.Dispose();
        }
    }
}
