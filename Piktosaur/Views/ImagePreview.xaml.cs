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
    private double scale = 1;

    public AppStateVM ViewModel => AppStateVM.Shared;

    public ImagePreview()
    {
        InitializeComponent();
        InitializeImage();

        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
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

        scale = 1;
        _isPanning = false;
        ImageTransform.ScaleX = 1;
        ImageTransform.ScaleY = 1;
        ImageTransform.TranslateX = 0;
        ImageTransform.TranslateY = 0;
    }

    private void PreviewImage_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        if (_isPanning) return;

        bool ctrlPressed = e.KeyModifiers.HasFlag(Windows.System.VirtualKeyModifiers.Control);
        if (!ctrlPressed) return;

        var point = e.GetCurrentPoint(sender as UIElement);
        int delta = point.Properties.MouseWheelDelta;

        // 120 is a typical 1 scrolling notch
        double zoomFactor = Math.Pow(1.1, delta / 120.0);
        scale *= zoomFactor;
        // prevent too much zooming
        scale = Math.Clamp(scale, 0.1, 10.0);

        ImageTransform.ScaleX = scale;
        ImageTransform.ScaleY = scale;

        e.Handled = true; // prevent the scroll from bubbling to a parent ScrollViewer
    }

    private bool _isPanning;
    private Point _lastPosition;

    private void PreviewImage_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (scale <= 1) return;

        _isPanning = true;
        _lastPosition = e.GetCurrentPoint(sender as UIElement).Position;
        if (sender is UIElement element)
        {
            element.CapturePointer(e.Pointer);
        }
    }

    private void PreviewImage_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isPanning) return;

        var pos = e.GetCurrentPoint(sender as UIElement).Position;

        double desiredX = ImageTransform.TranslateX + pos.X - _lastPosition.X;
        double desiredY = ImageTransform.TranslateY + pos.Y - _lastPosition.Y;

        double maxX = Math.Max(0, (PreviewImage.ActualWidth * scale - PreviewImage.ActualWidth) / 2);
        double maxY = Math.Max(0, (PreviewImage.ActualHeight * scale - PreviewImage.ActualHeight) / 2);

        ImageTransform.TranslateX = Math.Clamp(desiredX, -maxX, maxX);
        ImageTransform.TranslateY = Math.Clamp(desiredY, -maxY, maxY);

        _lastPosition = pos;
    }

    private void PreviewImage_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isPanning = false;
        if (sender is UIElement element)
        {
            element.ReleasePointerCapture(e.Pointer);
        }
    }
}
