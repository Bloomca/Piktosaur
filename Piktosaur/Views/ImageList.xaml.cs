using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Piktosaur.Models;
using Piktosaur.Services;
using Piktosaur.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Piktosaur.Views
{
    public sealed partial class ImageList : UserControl
    {
        public ObservableCollection<FolderWithImages> Folders => ImageQueryService.Shared.Folders;

        private CancellationTokenSource cancellationTokenSource;

        private readonly ImagesListVM VM = new ImagesListVM(AppStateVM.Shared);

        private bool isDisposed = false;

        public ImageList()
        {
            cancellationTokenSource = new CancellationTokenSource();
            InitializeComponent();
            LoadImages();
        }

        private void LoadImages()
        {
            try
            {
                var task = VM.LoadImages();

                task.ContinueWith(async (_) =>
                {
                    // small delay to guarantee that all data source is loaded
                    await Task.Delay(250);
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        await FocusSelectedItem();
                        AppStateVM.Shared.isLocked = false;
                    });
                });
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

        private async Task FocusSelectedItem()
        {
            // Retry until container is available
            for (int i = 0; i < 10; i++)
            {
                if (isDisposed) { return; }
                if (GridViewElement.Items.Count > 0)
                {
                    var selectedImagePath = AppStateVM.Shared.SelectedImagePath;
                    if (selectedImagePath == null) {
                        if (GridViewElement.Items[0] is not ImageResult firstImageItem) return;
                        var firstItem = GridViewElement.ContainerFromIndex(0) as GridViewItem;
                        GridViewElement.SelectedItem = firstImageItem;
                        if (SelectContainer(firstItem)) { return; }
                    } else
                    {
                        for (var index = 0; index < GridViewElement.Items.Count; index++)
                        {
                            if (GridViewElement.Items[index] is not ImageResult imageItem) return;
                            if (imageItem.Path == selectedImagePath)
                            {
                                if (index != 0)
                                {
                                    /// if the index is not 0, we should scroll the element into the view,
                                    /// otherwise it might not be realized due to virtualization
                                    GridViewElement.ScrollIntoView(imageItem, ScrollIntoViewAlignment.Default);

                                    // wait a small amount so that the scrolled element will be realized
                                    await Task.Delay(50);
                                }

                                var container = GridViewElement.ContainerFromIndex(index) as GridViewItem;
                                if (container != null)
                                {
                                    AppStateVM.Shared.isLocked = false;
                                    GridViewElement.SelectedItem = imageItem;
                                    if (SelectContainer(container)) { return; }
                                }
                            }
                        }
                    }
                }
                await Task.Delay(150);
            }
        }

        private bool SelectContainer(GridViewItem? item)
        {
            if (item != null)
            {
                item.Focus(FocusState.Keyboard);
                return true;
            }

            return false;
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppStateVM.Shared.isLocked) return;

            if (sender is not GridView gridView) return;
            if (gridView.SelectedItem is not ImageResult selectedItem) return;

            AppStateVM.Shared.SelectImage(selectedItem.Path);
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
