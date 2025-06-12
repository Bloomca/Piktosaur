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

        public static string[] ImageExtensions = [".jpg", ".jpeg", ".png"];

        public Search(ThumbnailGeneration thumbnailGeneration)
        {
            this.thumbnailGeneration = thumbnailGeneration;
        }

        public ImagesData GetImages(string path)
        {
            var results = new SearchResults(path);
            ReadDirectory(path, results);

            return ConvertSearchResults(results, null);
        }

        private void ReadDirectory(string path, SearchResults searchResults)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException();
            }

            var files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);

                if (fileInfo == null || !fileInfo.Exists) { continue; }

                if (ImageExtensions.Contains(fileInfo.Extension.ToLowerInvariant()))
                {
                    // Check for cloud/offline attributes
                    var attributes = File.GetAttributes(file);
                    if (attributes.HasFlag(System.IO.FileAttributes.Offline) ||
                        attributes.HasFlag(System.IO.FileAttributes.ReparsePoint))
                    {
                        continue; // File is likely in cloud storage
                    }

                    searchResults.AddImage(file, thumbnailGeneration);
                }
            }

            var directories = Directory.GetDirectories(path);

            foreach (var directory in directories)
            {
                var directoryResults = new SearchResults(directory);
                searchResults.AddDirectory(directoryResults);
                ReadDirectory(directory, directoryResults);
            }
        }

        private static ImagesData ConvertSearchResults(SearchResults searchResults, ImagesData? imagesData)
        {
            // The top result can have empty images (so we render the top folder correctly)
            if (imagesData == null)
            {
                imagesData = new ImagesData(searchResults.Path, searchResults.Images);
            }

            foreach (var directoryResult in searchResults.Directories)
            {
                if (directoryResult.Images.Count == 0)
                {
                    // if there are no images, we are skipping that directory
                    ConvertSearchResults(directoryResult, imagesData);
                } else
                {
                    var subdirectoryResult = new ImagesData(directoryResult.Path, directoryResult.Images);
                    ConvertSearchResults(directoryResult, subdirectoryResult);
                    imagesData.addSubdirectoryImagesData(subdirectoryResult);
                }
            }

            return imagesData;
        }
    }
}
