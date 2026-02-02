using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Piktosaur.ViewModels;

namespace Piktosaur.Views
{
    public sealed partial class SlideshowControls : UserControl
    {
        private SlideshowVM? viewModel;

        public SlideshowControls()
        {
            InitializeComponent();
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
