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
                new PropertyMetadata(null));

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

        private void ThumbnailImage_Loaded(object sender, RoutedEventArgs e)
        {
            this.EffectiveViewportChanged += Item_EffectiveViewportChanged;
        }

        /// <summary>
        /// This function implements virtualization. When the item is close to be shown,
        /// it requests creating a custom thumbnail. The actual thumbnail request can be
        /// denied if there are too many requests coming in (usually in case of rapid scroll).
        /// </summary>
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
