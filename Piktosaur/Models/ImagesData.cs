using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piktosaur.Models
{
    /// <summary>
    /// This class represents the Images Data ready to be shown in the UI.
    /// It is pretty similar to `SearchResults`, but it flattens empty
    /// directories.
    /// </summary>
    public class ImagesData
    {
        public string DirectoryPath { get; }

        public List<ImagesData> SubdirectoriesImagesData { get; private set; } = [];

        public List<ImageResult> Results { get; private set; };

        public ImagesData(string directoryPath, List<ImageResult> results)
        {
            DirectoryPath = directoryPath;
            Results = results;
        }

        public void addSubdirectoryImagesData(ImagesData subdirectoryImagesData)
        {
            SubdirectoriesImagesData.Add(subdirectoryImagesData);
        }
    }
}
