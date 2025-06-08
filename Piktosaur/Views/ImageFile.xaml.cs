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

using Piktosaur.Models;
using Piktosaur.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Piktosaur.Views
{
    public sealed partial class ImageFile : UserControl
    {
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(
                nameof(Image),
                typeof(ImageResult),
                typeof(ImageFile),
                new PropertyMetadata(null, OnImageChanged));

        public ImageResult Image
        {
            get => (ImageResult)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public ImageFile()
        {
            InitializeComponent();
        }

        private static void OnImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var control = d as ImageFile;
                var imageResult = e.NewValue as ImageResult;

                if (control == null || imageResult?.Thumbnail == null) return;

                control.ThumbnailImage.Source = imageResult.Thumbnail;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnImageChanged error: {ex}");
            }
        }

        public void RefreshThumbnail()
        {
            if (Image?.Thumbnail == null) return;
            ThumbnailImage.Source = Image.Thumbnail;
        }

        private void ThumbnailImage_Loaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            element.EffectiveViewportChanged += Item_EffectiveViewportChanged;
        }

        private async void Item_EffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            if (args.BringIntoViewDistanceX < 100 && args.BringIntoViewDistanceY < 100)
            {
                await Image.GenerateThumbnail();
                RefreshThumbnail();

                sender.EffectiveViewportChanged -= Item_EffectiveViewportChanged;
            }
        }
    }
}
