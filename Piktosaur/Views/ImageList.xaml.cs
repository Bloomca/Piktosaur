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
        }

        private async void LoadImages()
        {
            try
            {
                var folders = new List<FolderWithImages>();
                var currentQuery = AppStateVM.Shared.SelectedQuery;
                var result = Search.GetImages(currentQuery.Folders[0]);
                var images = result.Results;

                List<Task> thumbnailTasks = [];
                foreach (var image in images.Take(10))
                {
                    thumbnailTasks.Add(image.GenerateThumbnail());
                }

                if (images.Count > 0)
                {
                    AppStateVM.Shared.SelectImage(images[0].Path);
                }

                var ProgressElement = new ProgressBar
                {
                    IsIndeterminate = true,
                    ShowError = false,
                    ShowPaused = false
                };

                ContainerElement.Children.Add(ProgressElement);

                await Task.WhenAll(thumbnailTasks);

                ContainerElement.Children.Remove(ProgressElement);

                var folderName = System.IO.Path.GetFileName(result.DirectoryPath);
                var folderWithImages = new FolderWithImages(folderName, images);

                folders.Add(folderWithImages);

                HandleSubdirectories(result.SubdirectoriesImagesData, folders);

                ImagesByFolder.Source = folders;

                // small delay to guarantee that grid view will be properly focused
                // and the keyboard navigation will work immediately
                await Task.Delay(50);
                this.DispatcherQueue.TryEnqueue(() => GridViewElement.Focus(FocusState.Keyboard));
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

        private void ToggleGroupClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.DataContext is not FolderWithImages group) return;

            group.ToggleExpanded();
        }
    }
}
