using System;
using System.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
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
        private static readonly TimeSpan ControlsHideDelay = TimeSpan.FromSeconds(3);

        private readonly SlideshowVM viewModel;
        private AppWindow? appWindow;
        private DispatcherQueueTimer? controlsHideTimer;

        public SlideshowWindow()
        {
            InitializeComponent();

            viewModel = new SlideshowVM();
            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            Controls.SetViewModel(viewModel);

            Closed += OnWindowClosed;

            SetupControlsHideTimer();
            UpdateImage();
            SetFullScreen();
        }

        private void OnWindowClosed(object sender, WindowEventArgs args)
        {
            controlsHideTimer?.Stop();
            viewModel.Dispose();
        }

        private void SetupControlsHideTimer()
        {
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            if (dispatcherQueue == null) return;

            controlsHideTimer = dispatcherQueue.CreateTimer();
            controlsHideTimer.Interval = ControlsHideDelay;
            controlsHideTimer.IsRepeating = false;
            controlsHideTimer.Tick += (s, e) =>
            {
                if (!Controls.IsPointerOver)
                {
                    Controls.Hide();
                }
            };
            controlsHideTimer.Start();
        }

        private void ShowControlsAndResetTimer()
        {
            Controls.Show();
            controlsHideTimer?.Stop();
            controlsHideTimer?.Start();
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
            viewModel.TogglePlayPause();
            ShowControlsAndResetTimer();
            args.Handled = true;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            ShowControlsAndResetTimer();
        }
    }
}
