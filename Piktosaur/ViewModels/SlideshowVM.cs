using System;
using Piktosaur.Services;

namespace Piktosaur.ViewModels
{
    public class SlideshowVM : BaseViewModel
    {
        private string[] imagePaths;
        private int currentIndex;

        public string? CurrentImagePath
        {
            get => currentIndex >= 0 && currentIndex < imagePaths.Length ? imagePaths[currentIndex] : null;
        }

        public SlideshowVM()
        {
            // Get snapshot of current images
            imagePaths = ImageQueryService.Shared.GetImagePathsSnapshot();

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
        }

        public bool HasImages => imagePaths.Length > 0;

        public void NextImage()
        {
            if (imagePaths.Length == 0 || imagePaths.Length == 1) return;

            currentIndex = (currentIndex + 1) % imagePaths.Length;
            OnPropertyChanged(nameof(CurrentImagePath));
        }

        public void PreviousImage()
        {
            if (imagePaths.Length == 0 || imagePaths.Length == 1) return;

            currentIndex = (currentIndex - 1 + imagePaths.Length) % imagePaths.Length;
            OnPropertyChanged(nameof(CurrentImagePath));
        }
    }
}
