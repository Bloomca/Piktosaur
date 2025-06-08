using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using static System.Net.Mime.MediaTypeNames;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Piktosaur.Views
{
    public sealed partial class ImageList : UserControl
    {
        public ImageList()
        {
            InitializeComponent();

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

        private void LoadImages()
        {
            try
            {
                var folders = new List<FolderWithImages>();
                var currentQuery = AppStateVM.Shared.SelectedQuery;
                var result = Search.GetImages(currentQuery.Folders[0]);
                var images = result.Results;

                if (images.Count > 0)
                {
                    AppStateVM.Shared.SelectImage(images[0].Path);
                }

                var folderName = System.IO.Path.GetFileName(result.DirectoryPath);
                var folderWithImages = new FolderWithImages(folderName, images);

                folders.Add(folderWithImages);

                HandleSubdirectories(result.SubdirectoriesImagesData, folders);

                ImagesByFolder.Source = folders;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during LoadImages: {ex}");
            }
        }

        private void HandleSubdirectories(List<ImagesData> imagesDataList, List<FolderWithImages> folders)
        {
            foreach (var imagesData in imagesDataList)
            {
                var folderName = System.IO.Path.GetFileName(imagesData.DirectoryPath);
                var folderWithImages = new FolderWithImages(folderName, imagesData.Results);

                folders.Add(folderWithImages);

                HandleSubdirectories(imagesData.SubdirectoriesImagesData, folders);
            }
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

        private async void GridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                // TODO: cleanup
                return;
            }

            if (args.Phase == 0)
            {
                // for now, do nothing, just register for the next phase
                args.RegisterUpdateCallback(GridView_ContainerContentChanging);
                args.Handled = true;
            }

            if (args.Phase == 1)
            {
                if (args.Item is not ImageResult imageItem)
                {
                    System.Diagnostics.Trace.WriteLine("Item is not ImageResult");
                    return;
                }

                System.Diagnostics.Trace.WriteLine($"Generating thumbnail for: {imageItem.Path}");

                try
                {
                    await imageItem.GenerateThumbnail();
                    System.Diagnostics.Trace.WriteLine($"Thumbnail generated for: {imageItem.Path}");
                    var imageFile = args.ItemContainer.ContentTemplateRoot as ImageFile;
                    imageFile?.RefreshThumbnail();
                    args.Handled = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Could not refresh image thumbnail: {ex}");
                }
                
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
