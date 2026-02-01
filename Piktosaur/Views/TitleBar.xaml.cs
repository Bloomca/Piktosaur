using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Piktosaur.Models;
using Piktosaur.Services;
using Piktosaur.ViewModels;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Piktosaur.Views
{
    public sealed partial class TitleBar : UserControl
    {
        public AppStateVM ViewModel => AppStateVM.Shared;
        public TitleBar()
        {
            InitializeComponent();

            SetFlyoutItems();

            ViewModel.Queries.CollectionChanged += OnQueriesCollectionChanged;
            ImageQueryService.Shared.Folders.CollectionChanged += OnFoldersCollectionChanged;
            SlideshowManager.Shared.PropertyChanged += OnSlideshowManagerPropertyChanged;

            // Initial state - disable until images are loaded
            UpdateSlideshowButtonState();
        }

        private void OnQueriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SetFlyoutItems();
        }

        private void OnFoldersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateSlideshowButtonState();
        }

        private void OnSlideshowManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SlideshowManager.IsOpen))
            {
                UpdateSlideshowButtonAppearance();
            }
        }

        /// <summary>
        /// It is not possible to use <DataTemplate> component
        /// for FlyoutItems, so we assign them manually.
        /// 
        /// This is not the most performant solution, as it clears
        /// them all instead of reacting to individual changes of
        /// the collection, but it should be negligible here.
        /// </summary>
        private void SetFlyoutItems()
        {
            MenuElement.Items.Clear();
            foreach (var query in ViewModel.Queries)
            {
                var savedQuery = query;
                var flyoutItem = new MenuFlyoutItem {
                    Text = query.Name,
                    Icon = new FontIcon { Glyph = "\uE8D5" }
                };
                flyoutItem.MaxWidth = 400;
                flyoutItem.Click += (sender, e) => ViewModel.SelectQuery(savedQuery);
                ToolTipService.SetToolTip(flyoutItem, query.Folder);

                MenuElement.Items.Add(flyoutItem);
            }

            var openFolderFlyoutItem = new MenuFlyoutItem {
                Text = "Open folder",
                Icon = new FontIcon { Glyph = "\uE8F4" }
            };
            openFolderFlyoutItem.Click += (sender, e) => HandleOpenFolderClick();

            MenuElement.Items.Add(openFolderFlyoutItem);
        }

        private async void HandleOpenFolderClick()
        {
            var folderPicker = new FolderPicker();
            InitializeWithWindow.Initialize(folderPicker, App.MainWindowHandle);

            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                AppStateVM.Shared.AddFolderQuery(folder);
            }
        }

        private async void InfoButtonClick(object sender, RoutedEventArgs e)
        {
            var aboutDialogue = new AboutDialogue();
            aboutDialogue.XamlRoot = this.Content.XamlRoot;
            await aboutDialogue.ShowAsync();
        }

        private void OpenFileExplorerClick(object sender, RoutedEventArgs e)
        {
            var path = AppStateVM.Shared.SelectedImagePath;

            if (path != null)
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            } else
            {
                var query = AppStateVM.Shared.SelectedQuery;
                var folder = query.Folder;

                if (folder != null) {
                    Process.Start("explorer.exe", $"\"{folder}\"");
                }
            }
        }

        private void SlideshowButtonClick(object sender, RoutedEventArgs e)
        {
            if (SlideshowManager.Shared.IsOpen)
            {
                SlideshowManager.Shared.Close();
            }
            else
            {
                // Check if there are any images to show
                if (ImageQueryService.Shared.Folders.Count == 0)
                {
                    return;
                }

                SlideshowManager.Shared.Open();
            }
        }

        /// <summary>
        /// Updates the slideshow button enabled state based on whether there are images.
        /// </summary>
        private void UpdateSlideshowButtonState()
        {
            SlideshowButton.IsEnabled = SlideshowManager.Shared.IsOpen || ImageQueryService.Shared.Folders.Count > 0;
        }

        /// <summary>
        /// Updates the slideshow button icon and tooltip based on whether slideshow is open.
        /// </summary>
        private void UpdateSlideshowButtonAppearance()
        {
            if (SlideshowManager.Shared.IsOpen)
            {
                SlideshowIcon.Glyph = "\uE71A"; // Stop icon
                ToolTipService.SetToolTip(SlideshowButton, "Stop slideshow");
            }
            else
            {
                SlideshowIcon.Glyph = "\uE768"; // Play icon
                ToolTipService.SetToolTip(SlideshowButton, "Start slideshow");
            }

            UpdateSlideshowButtonState();
        }
    }
}
