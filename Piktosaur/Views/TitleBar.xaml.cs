using System;
using System.Collections.Generic;
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
using Windows.Foundation;
using Windows.Foundation.Collections;

using Piktosaur.ViewModels;

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
        }

        private void SetFlyoutItems()
        {
            foreach (var query in ViewModel.Queries)
            {
                var savedQuery = query;
                var flyoutItem = new MenuFlyoutItem { Text = query.Name };
                flyoutItem.Click += (sender, e) => ViewModel.SelectQuery(savedQuery);

                MenuElement.Items.Add(flyoutItem);
            }
        }
    }
}
