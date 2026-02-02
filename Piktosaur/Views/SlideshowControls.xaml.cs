using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Piktosaur.ViewModels;

namespace Piktosaur.Views
{
    public sealed partial class SlideshowControls : UserControl
    {
        private static readonly TimeSpan FadeOutDuration = TimeSpan.FromMilliseconds(200);

        private SlideshowVM? viewModel;
        private bool isPointerOver;

        public bool IsPointerOver => isPointerOver;

        public SlideshowControls()
        {
            InitializeComponent();
            PointerEntered += (s, e) => isPointerOver = true;
            PointerExited += (s, e) => isPointerOver = false;
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
            Opacity = 1;
        }

        public void Hide()
        {
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(FadeOutDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var storyboard = new Storyboard();
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);
            storyboard.Completed += (s, e) => Visibility = Visibility.Collapsed;
            storyboard.Begin();
        }

        public void SetViewModel(SlideshowVM vm)
        {
            if (viewModel != null)
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            viewModel = vm;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdatePlayPauseIcon();

            if (viewModel.IsPlaying)
            {
                ProgressBar.Start();
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SlideshowVM.IsPlaying))
            {
                UpdatePlayPauseIcon();
                UpdateProgressBar();
            }
            else if (e.PropertyName == nameof(SlideshowVM.CurrentImagePath))
            {
                if (viewModel?.IsPlaying == true)
                {
                    ProgressBar.Restart();
                } else
                {
                    ProgressBar.Restart();
                    ProgressBar.Pause();
                }
            }
        }

        private void UpdatePlayPauseIcon()
        {
            if (viewModel == null) return;
            PlayPauseIcon.Glyph = viewModel.IsPlaying ? "\uE769" : "\uE768";
        }

        private void UpdateProgressBar()
        {
            if (viewModel == null) return;

            if (viewModel.IsPlaying)
            {
                ProgressBar.Resume();
            }
            else
            {
                ProgressBar.Pause();
            }
        }

        private void OnFirstClick(object sender, RoutedEventArgs e)
        {
            viewModel?.FirstImage();
        }

        private void OnPreviousClick(object sender, RoutedEventArgs e)
        {
            viewModel?.PreviousImage();
        }

        private void OnPlayPauseClick(object sender, RoutedEventArgs e)
        {
            viewModel?.TogglePlayPause();
        }

        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            viewModel?.NextImage();
        }

        private void OnLastClick(object sender, RoutedEventArgs e)
        {
            viewModel?.LastImage();
        }
    }
}
