using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piktosaur.Services;

namespace Piktosaur.Models
{
    public class SearchResults
    {
        public List<ImageResult> Images { get; private set; } = [];

        public string Path { get; }

        public SearchResults(string path)
        {
            Path = path;
        }

        public void AddImage(string path, ThumbnailGeneration thumbnailGeneration)
        {
            Images.Add(new ImageResult(path, thumbnailGeneration));
        }
    }
}
