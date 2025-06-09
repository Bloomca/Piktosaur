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

using Piktosaur.Services;
using Piktosaur.Views;
using Piktosaur.ViewModels;

using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Piktosaur
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private ImageList? imageList;
        public Piktosaur.Views.TitleBar TitleBar => CustomTitleBar;
        public MainWindow()
        {
            InitializeComponent();
            ReloadImagesList();

            AppStateVM.Shared.PropertyChanged += Shared_PropertyChanged;
        }

        private void Shared_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppStateVM.Shared.SelectedQuery))
            {
                ReloadImagesList();
            }
        }

        private void ReloadImagesList()
        {
            if (imageList != null) {
                ContainerElement.Children.Remove(imageList);
            }
            var ImagesListComponent = new ImageList();
            imageList = ImagesListComponent;
            Grid.SetRow(ImagesListComponent, 1);
            ContainerElement.Children.Add(ImagesListComponent);
        }
    }
}
