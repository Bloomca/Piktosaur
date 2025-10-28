using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using System.Runtime.InteropServices;

using Piktosaur.Models;
using System.Collections.ObjectModel;
using Piktosaur.ViewModels;
using Microsoft.UI.Dispatching;
using System.Diagnostics;

namespace Piktosaur.Services
{
    /// <summary>
    /// Implements search of all images within a passed directory path.
    /// The important part here is that the search is recursive, and even
    /// a folder has no images, it will keep going until it fully exhausts
    /// the path.
    /// 
    /// This architecture allows to render _all_ photos inline, without
    /// the requirement on clicking on individual folders multiple times.
    /// </summary>
    public class Search
    {
        private ThumbnailGeneration thumbnailGeneration;

        private DispatcherQueue dispatcherQueue;

        private ObservableCollection<FolderWithImages> folders;

        public static string[] ImageExtensions = [".jpg", ".jpeg", ".png"];

        public Search(ThumbnailGeneration thumbnailGeneration, ObservableCollection<FolderWithImages> _folders)
        {
            this.thumbnailGeneration = thumbnailGeneration;
            this.folders = _folders;

            this.dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        public void GetImages(string path)
        {
            _ = _GetImages(path);
        }

        private async Task _GetImages(string path)
        {
              await Task.Run(() => GetFolderWithImages(path));
        }

        private void GetFolderWithImages(string path)
        {
            var searchResult = new SearchResults(path);
            var imagesData = new ImagesData(path, searchResult.Images);
            var folderName = System.IO.Path.GetFileName(path);
            var folderWithImages = new FolderWithImages(folderName);

            var directories = ReadDirectory(path, folderWithImages);

            foreach (var directory in directories)
            {
                GetFolderWithImages(directory);
            }

            return;
        }

        private IEnumerable<string> ReadDirectory(string path, FolderWithImages folder)
        {
            if (!Directory.Exists(path))
            {
                return [];
            }

            bool hasImages = false;

            try
            {
                var files = Directory.EnumerateFiles(path);

                try
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(file);

                            if (!fileInfo.Exists) { continue; }

                            if (ImageExtensions.Contains(fileInfo.Extension.ToLowerInvariant()))
                            {
                                // Check for cloud/offline attributes
                                var attributes = fileInfo.Attributes;
                                if (attributes.HasFlag(System.IO.FileAttributes.Offline) ||
                                    attributes.HasFlag(System.IO.FileAttributes.ReparsePoint))
                                {
                                    continue; // File is likely in cloud storage
                                }

                                hasImages = true;

                                dispatcherQueue.TryEnqueue(() =>
                                {
                                    folder.AddImage(new ImageResult(file, thumbnailGeneration));
                                });
                            }
                        }
                        catch
                        {
                            // if for some reason we couldn't get FileInfo, just skip the file
                            continue;
                        }
                    }
                }
                catch
                {
                    // pass
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
                // pass
            }

            // only add folder if it has some images
            if (hasImages)
            {
                dispatcherQueue.TryEnqueue(() =>
                {
                    folders.Add(folder);
                });
            }

            try
            {
                return Directory.EnumerateDirectories(path);
            } catch
            {
                return [];
            }
            
        }
    }
}
