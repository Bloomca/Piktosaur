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
        public ObservableCollection<FolderWithImages> Folders { get; } = new();

        private CancellationTokenSource cancellationTokenSource;

        private readonly ImagesListVM VM = new ImagesListVM(AppStateVM.Shared);

        private bool isDisposed = false;

        public ImageList()
        {
            cancellationTokenSource = new CancellationTokenSource();
            InitializeComponent();
            LoadImages();
        }

        private async void LoadImages()
        {
            try
            {
                await VM.LoadImages(Folders);

                // small delay to guarantee that grid view will be properly focused
                // and the keyboard navigation will work immediately
                await Task.Delay(50);
                this.DispatcherQueue.TryEnqueue(async () => await FocusFirstItem());
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

        private async Task FocusFirstItem()
        {
            // Retry until container is available
            for (int i = 0; i < 10; i++)
            {
                if (isDisposed) { return; }
                if (GridViewElement.Items.Count > 0)
                {
                    var firstItem = GridViewElement.ContainerFromIndex(0) as GridViewItem;
                    if (firstItem != null)
                    {
                        firstItem.Focus(FocusState.Keyboard);
                        return;
                    }

                }
                await Task.Delay(100);
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

        private void GridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
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
                if (args.Item is not ImageResult imageItem) return;
                _ = imageItem.GenerateThumbnail(cancellationTokenSource.Token);
                args.Handled = true;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!isDisposed)
            {
                cancellationTokenSource.Cancel();
                VM.Dispose();
                isDisposed = true;
            }
        }
    }
}
