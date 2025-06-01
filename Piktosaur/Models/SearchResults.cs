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
        public IEnumerable<ImageResult> Images { get; private set; } = [];

        public IEnumerable<SearchResults> Directories { get; private set; } = [];

        public string Path { get; }

        public SearchResults(string path)
        {
            Path = path;
        }

        public void AddImage(string path)
        {
            Images.Append(new ImageResult(path));
        }

        public void AddDirectory(SearchResults directoryResults)
        {
            Directories.Append(directoryResults);
        }
    }
}
