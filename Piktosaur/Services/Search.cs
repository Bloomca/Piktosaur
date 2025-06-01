using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

using Piktosaur.Models;

namespace Piktosaur.Services
{
    public class Search
    {
        public static string[] ImageExtensions = [".jpg", ".jpeg", ".png"];
        public static string GetPicturesFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        public static ImagesData GetImages(string path)
        {
            var results = new SearchResults(path);
            ReadDirectory(path, results);

            return ConvertSearchResults(results, null);
        }

        private static void ReadDirectory(string path, SearchResults searchResults)
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
                    searchResults.AddImage(file);
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
