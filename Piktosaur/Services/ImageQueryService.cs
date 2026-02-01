using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Piktosaur.Models;
using Piktosaur.ViewModels;

namespace Piktosaur.Services
{
    /// <summary>
    /// Singleton service that owns the image discovery logic and caches results.
    /// Both the main window (ImagesListVM) and the slideshow can depend on this service.
    /// </summary>
    public class ImageQueryService : IDisposable
    {
        public static ImageQueryService Shared = new ImageQueryService();

        private IThumbnailGenerator thumbnailGenerator;
        private bool ownsThumbnailGenerator;
        private bool useDispatcherQueue;
        private bool isDisposed = false;

        /// <summary>
        /// The collection of folders with images from the current query.
        /// Consumers can bind directly to this collection.
        /// </summary>
        public ObservableCollection<FolderWithImages> Folders { get; } = new();

        private ImageQueryService()
        {
            thumbnailGenerator = new ThumbnailGenerator();
            ownsThumbnailGenerator = true;
            useDispatcherQueue = true;
        }

        /// <summary>
        /// Constructor for testing - allows injecting a mock thumbnail generator.
        /// DispatcherQueue is not used in test mode.
        /// </summary>
        public ImageQueryService(IThumbnailGenerator thumbnailGenerator)
        {
            this.thumbnailGenerator = thumbnailGenerator;
            ownsThumbnailGenerator = false;
            useDispatcherQueue = false;
        }

        /// <summary>
        /// Executes a query, clearing previous results and progressively loading new images.
        /// Returns a Task that completes when the search is done.
        /// </summary>
        public Task ExecuteQuery(Query query)
        {
            ClearFolders();

            DispatcherQueue? dispatcherQueue = useDispatcherQueue ? DispatcherQueue.GetForCurrentThread() : null;
            var search = new Search(thumbnailGenerator, Folders, dispatcherQueue);
            return search.GetImages(query.Folder);
        }

        /// <summary>
        /// Returns a snapshot of current image paths for use by the slideshow.
        /// The slideshow gets its own copy, independent of future query changes.
        /// </summary>
        public string[] GetImagePathsSnapshot()
        {
            var paths = new List<string>();
            foreach (var folder in Folders)
            {
                foreach (var image in folder.Images)
                {
                    paths.Add(image.Path);
                }
            }
            return paths.ToArray();
        }

        /// <summary>
        /// Generates thumbnails for the first batch of images in the first folder.
        /// </summary>
        public async Task GenerateInitialThumbnails(CancellationToken cancellationToken)
        {
            if (Folders.Count == 0) return;

            var folder = Folders[0];
            if (folder == null) return;

            var thumbnailTasks = new List<Task>();

            foreach (var image in folder.Images)
            {
                if (thumbnailTasks.Count >= 25) break;
                thumbnailTasks.Add(image.GenerateThumbnail(cancellationToken));
            }

            if (cancellationToken.IsCancellationRequested) return;

            await Task.WhenAll(thumbnailTasks);
        }

        private void ClearFolders()
        {
            foreach (var folder in Folders)
            {
                folder?.Dispose();
            }
            Folders.Clear();
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            ClearFolders();
            if (ownsThumbnailGenerator && thumbnailGenerator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
