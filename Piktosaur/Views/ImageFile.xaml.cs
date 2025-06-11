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
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Piktosaur.Views
{
    public sealed partial class ImageFile : UserControl
    {
        public bool _unloaded = false;

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

        private CancellationTokenSource? cancellationTokenSource;

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

                control.EffectiveViewportChanged -= control.Item_EffectiveViewportChanged;
                if (control._unloaded == true) return;
                control.ThumbnailImage.Source = imageResult.Thumbnail;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnImageChanged error: {ex}");
            }
        }

        private void RefreshThumbnail()
        {
            if (_unloaded == true) return;
            if (cancellationTokenSource != null && cancellationTokenSource.Token.IsCancellationRequested) return;
            if (Image?.Thumbnail == null)
            {
                // if we are here, it means that thumbnail generation didn't succeed
                // so we need to "reset" the state so it can be fetched again
                cancellationTokenSource = null;
                this.EffectiveViewportChanged += Item_EffectiveViewportChanged;
                return;
            }
            ThumbnailImage.Source = Image.Thumbnail;
        }

        private void ThumbnailImage_Loaded(object sender, RoutedEventArgs e)
        {
            this.EffectiveViewportChanged += Item_EffectiveViewportChanged;
        }

        private async void Item_EffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            if (Image == null) return;
            if (_unloaded == true || Image.Thumbnail != null)
            {
                this.EffectiveViewportChanged -= Item_EffectiveViewportChanged;
                return;
            }
            if (cancellationTokenSource != null) return;
            if (args.BringIntoViewDistanceX < 100 && args.BringIntoViewDistanceY < 100)
            {
                this.EffectiveViewportChanged -= Item_EffectiveViewportChanged;
                cancellationTokenSource = new CancellationTokenSource();
                try
                {
                    await Image.GenerateThumbnail(cancellationTokenSource.Token);

                    if (!cancellationTokenSource.Token.IsCancellationRequested && !_unloaded)
                    {
                        RefreshThumbnail();
                    }
                }
                catch (OperationCanceledException)
                {
                    // do nothing
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Failed to generate thumbnail");
                }
            }
        }

        private void ThumbnailImage_Unloaded(object sender, RoutedEventArgs e)
        {
            _unloaded = true;
            cancellationTokenSource?.Cancel();
            //cancellationTokenSource?.Dispose();
            this.EffectiveViewportChanged -= Item_EffectiveViewportChanged;
        }
    }
}
