using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Piktosaur.Models;
using Piktosaur.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        }

        private void OnQueriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SetFlyoutItems();
        }

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
                flyoutItem.Click += (sender, e) => ViewModel.SelectQuery(savedQuery);

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
    }
}
