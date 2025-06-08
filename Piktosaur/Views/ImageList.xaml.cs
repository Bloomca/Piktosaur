using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Piktosaur.Models;
using Piktosaur.Services;
using Piktosaur.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Piktosaur.Views
{
    public sealed partial class ImageList : UserControl
    {
        public CollectionViewSource ImagesByFolder { get; private set; }

        public ImageList()
        {
            InitializeComponent();

            ImagesByFolder = new CollectionViewSource
            {
                IsSourceGrouped = true,
                ItemsPath = new PropertyPath("Images")
            };

            LoadImages();

            AppStateVM.Shared.PropertyChanged += Shared_PropertyChanged;
        }

        private void Shared_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppStateVM.Shared.SelectedQuery))
            {
                LoadImages();
            }
        }

        private async void LoadImages()
        {
            var Folders = new List<FolderWithImages>();
            var currentQuery = AppStateVM.Shared.SelectedQuery;
            var result = Search.GetImages(currentQuery.Folders[0]);
            var images = result.Results;
            List<Task> thumbnailTasks = [];
            // pre-generate first 15 image thumbnails
            foreach (var image in images.Take(15))
            {
                thumbnailTasks.Add(image.GenerateThumbnail());
            }

            if (images.Count > 0)
            {
                AppStateVM.Shared.SelectImage(images[0].Path);
            }

            await Task.WhenAll(thumbnailTasks);

            var folderName = System.IO.Path.GetFileName(result.DirectoryPath);
            var folderWithImages = new FolderWithImages(folderName, images);

            Folders.Add(folderWithImages);

            ImagesByFolder.Source = Folders;
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var gridView = sender as GridView;
            var selectedItem = gridView?.SelectedItem as ImageResult;

            if (selectedItem != null)
            {
                AppStateVM.Shared.SelectImage(selectedItem.Path);
            }
        }
    }

    public class FolderWithImages
    {
        public string Name { get; }

        public ObservableCollection<ImageResult> Images { get; }

        public FolderWithImages(string name, IReadOnlyList<ImageResult> images)
        {
            Name = name;
            Images = new ObservableCollection<ImageResult>(images);
        }
    }
}
