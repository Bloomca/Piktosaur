using System;
using System.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Piktosaur.ViewModels;
using WinRT.Interop;

namespace Piktosaur.Views
{
    public sealed partial class SlideshowWindow : Window
    {
        private readonly SlideshowVM viewModel;
        private AppWindow? appWindow;

        public SlideshowWindow()
        {
            InitializeComponent();

            viewModel = new SlideshowVM();
            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            UpdateImage();
            SetFullScreen();
        }

        private void SetFullScreen()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SlideshowVM.CurrentImagePath))
            {
                UpdateImage();
            }
        }

        private void UpdateImage()
        {
            var path = viewModel.CurrentImagePath;
            if (path != null)
            {
                var bitmap = new BitmapImage();
                bitmap.UriSource = new Uri(path);
                SlideshowImage.Source = bitmap;
            }
            else
            {
                SlideshowImage.Source = null;
            }
        }

        private void OnEscapePressed(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            Close();
            args.Handled = true;
        }

        private void OnLeftPressed(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            viewModel.PreviousImage();
            args.Handled = true;
        }

        private void OnRightPressed(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            viewModel.NextImage();
            args.Handled = true;
        }

        private void OnSpacePressed(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            // TODO: Step 8 - Toggle pause
            args.Handled = true;
        }
    }
}
