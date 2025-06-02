using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Piktosaur.Models;
using Piktosaur.Services;

namespace Piktosaur.ViewModels
{
    public class AppStateVM : BaseViewModel
    {
        public static Query[] DefaultQueries = [
            new Query("My Pictures", [Search.GetPicturesFolder()]),
            new Query("Downloads", [Search.GetDownloadsFolder()])
        ];

        public static AppStateVM Shared = new AppStateVM();

        private string? selectedImagePath;

        public string? SelectedImagePath {
            get => selectedImagePath;
            private set => SetProperty(ref selectedImagePath, value);
        }

        public bool SelectImage(string imagePath)
        {
            if (!System.IO.File.Exists(imagePath))
            {
                return false;
            }

            SelectedImagePath = imagePath;
            return true;
        }

        private Query selectedQuery = DefaultQueries[0];

        public Query SelectedQuery {
            get => selectedQuery;
            private set => SetProperty(ref selectedQuery, value);
        }

        public ObservableCollection<Query> Queries { get; } = new(DefaultQueries);

        public bool SelectQuery(Query query)
        {
            if (Queries.Contains(query))
            {
                SelectedQuery = query;
                return true;
            }

            return false;
        }
    }
}
