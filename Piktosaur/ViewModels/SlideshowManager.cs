using System;
using Piktosaur.Views;

namespace Piktosaur.ViewModels
{
    /// <summary>
    /// Manages the slideshow window lifecycle.
    /// </summary>
    public class SlideshowManager : BaseViewModel
    {
        public static SlideshowManager Shared = new SlideshowManager();

        private SlideshowWindow? slideshowWindow;

        public bool IsOpen => slideshowWindow != null;

        private SlideshowManager() { }

        public void Open()
        {
            if (slideshowWindow != null)
            {
                // Already open, just activate it
                slideshowWindow.Activate();
                return;
            }

            slideshowWindow = new SlideshowWindow();
            slideshowWindow.Closed += OnSlideshowWindowClosed;
            slideshowWindow.Activate();

            OnPropertyChanged(nameof(IsOpen));
        }

        public void Close()
        {
            if (slideshowWindow == null) return;

            slideshowWindow.Close();
            // OnSlideshowWindowClosed will handle cleanup
        }

        private void OnSlideshowWindowClosed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
        {
            if (slideshowWindow != null)
            {
                slideshowWindow.Closed -= OnSlideshowWindowClosed;
                slideshowWindow = null;
            }

            OnPropertyChanged(nameof(IsOpen));
        }
    }
}
