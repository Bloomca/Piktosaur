using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
        private readonly ImagesListVM VM = new ImagesListVM(AppStateVM.Shared);

        public ImageList()
        {
            InitializeComponent();

            LoadImages();
        }

        private async void LoadImages()
        {
            try
            {
                var folders = await VM.LoadImages();
                ImagesByFolder.Source = folders;

                // small delay to guarantee that grid view will be properly focused
                // and the keyboard navigation will work immediately
                await Task.Delay(50);
                this.DispatcherQueue.TryEnqueue(FocusFirstItem);
            }
            catch (OperationCanceledException)
            {
                // pass, nothing to do in this case
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during LoadImages: {ex}");
            }
        }

        private void FocusFirstItem()
        {
            if (GridViewElement.Items.Count > 0)
            {
                var firstItem = GridViewElement.ContainerFromIndex(0) as GridViewItem;
                firstItem?.Focus(FocusState.Keyboard);
            }
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not GridView gridView) return;
            if (gridView.SelectedItem is not ImageResult selectedItem) return;

            if (selectedItem != null)
            {
                AppStateVM.Shared.SelectImage(selectedItem.Path);
            }
        }

        private void ToggleGroupClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.DataContext is not FolderWithImages group) return;

            group.ToggleExpanded();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            VM.ClearFoldersData();
        }
    }
}
