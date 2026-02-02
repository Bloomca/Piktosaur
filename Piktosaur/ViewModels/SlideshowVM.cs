using System;
using Microsoft.UI.Dispatching;
using Piktosaur.Services;

namespace Piktosaur.ViewModels
{
    public class SlideshowVM : BaseViewModel, IDisposable
    {
        public static readonly TimeSpan SlideshowInterval = TimeSpan.FromSeconds(5);

        private readonly Random random = new();
        private string[] imagePaths;
        private int currentIndex;
        private DispatcherQueueTimer? timer;
        private bool isDisposed;
        private bool isPlaying = true;

        public string? CurrentImagePath
        {
            get => currentIndex >= 0 && currentIndex < imagePaths.Length ? imagePaths[currentIndex] : null;
        }

        public SlideshowVM()
        {
            // Get snapshot of current images
            var skipCollapsed = SettingsVM.Shared.SkipCollapsedFolders;
            imagePaths = ImageQueryService.Shared.GetImagePathsSnapshot(skipCollapsed);

            // Find the starting index based on selected image
            var selectedPath = AppStateVM.Shared.SelectedImagePath;
            currentIndex = 0;

            if (selectedPath != null)
            {
                var index = Array.IndexOf(imagePaths, selectedPath);
                if (index >= 0)
                {
                    currentIndex = index;
                }
            }

            StartTimer();
        }

        public bool HasImages => imagePaths.Length > 0;

        public bool IsPlaying
        {
            get => isPlaying;
            private set => SetProperty(ref isPlaying, value);
        }

        public void NextImage()
        {
            if (imagePaths.Length == 0 || imagePaths.Length == 1) return;

            if (SettingsVM.Shared.UseRandomOrder)
            {
                currentIndex = random.Next(imagePaths.Length);
            }
            else
            {
                currentIndex = (currentIndex + 1) % imagePaths.Length;
            }
            OnPropertyChanged(nameof(CurrentImagePath));
            ResetTimer();
        }

        public void PreviousImage()
        {
            if (imagePaths.Length == 0 || imagePaths.Length == 1) return;

            if (SettingsVM.Shared.UseRandomOrder)
            {
                currentIndex = random.Next(imagePaths.Length);
            }
            else
            {
                currentIndex = (currentIndex - 1 + imagePaths.Length) % imagePaths.Length;
            }
            OnPropertyChanged(nameof(CurrentImagePath));
            ResetTimer();
        }

        public void FirstImage()
        {
            if (imagePaths.Length == 0) return;

            currentIndex = 0;
            OnPropertyChanged(nameof(CurrentImagePath));
            ResetTimer();
        }

        public void LastImage()
        {
            if (imagePaths.Length == 0) return;

            currentIndex = imagePaths.Length - 1;
            OnPropertyChanged(nameof(CurrentImagePath));
            ResetTimer();
        }

        public void TogglePlayPause()
        {
            if (timer == null) return;

            if (IsPlaying)
            {
                timer.Stop();
            }
            else
            {
                timer.Start();
            }
            IsPlaying = !IsPlaying;
        }

        private void ResetTimer()
        {
            if (timer == null || !IsPlaying) return;
            timer.Stop();
            timer.Start();
        }

        private void StartTimer()
        {
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            if (dispatcherQueue == null) return;

            timer = dispatcherQueue.CreateTimer();
            timer.Interval = SlideshowInterval;
            timer.Tick += (s, e) => NextImage();
            timer.Start();
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            timer?.Stop();
            timer = null;
        }
    }
}
