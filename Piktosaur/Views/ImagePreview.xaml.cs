using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Piktosaur.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Piktosaur.Views;

public sealed partial class ImagePreview : UserControl
{
    public AppStateVM ViewModel => AppStateVM.Shared;

    public ImagePreview()
    {
        InitializeComponent();
        InitializeImage();

        AppStateVM.Shared.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void InitializeImage()
    {
        var selectedImagePath = ViewModel.SelectedImagePath;

        if (selectedImagePath != null) AssignImageSource(selectedImagePath);
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.SelectedImagePath))
        {
            if (string.IsNullOrEmpty(ViewModel.SelectedImagePath))
            {
                PreviewImage.Source = null;
            }
            else
            {
                AssignImageSource(ViewModel.SelectedImagePath);
            }
        }
    }

    private void AssignImageSource(string imagePath)
    {
        var bitmap = new BitmapImage();
        bitmap.UriSource = new Uri(imagePath);
        PreviewImage.Source = bitmap;
    }
}
