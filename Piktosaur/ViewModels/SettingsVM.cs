namespace Piktosaur.ViewModels
{
    public enum SlideshowAnimation
    {
        None,
        Fade,
        Slide,
        Zoom
    }

    public class SettingsVM : BaseViewModel
    {
        public static SettingsVM Shared = new SettingsVM();

        private SlideshowAnimation slideshowAnimation = SlideshowAnimation.None;
        private bool useRandomOrder = false;
        private bool skipCollapsedFolders = false;

        public SlideshowAnimation SlideshowAnimation
        {
            get => slideshowAnimation;
            set => SetProperty(ref slideshowAnimation, value);
        }

        public bool UseRandomOrder
        {
            get => useRandomOrder;
            set => SetProperty(ref useRandomOrder, value);
        }

        public bool SkipCollapsedFolders
        {
            get => skipCollapsedFolders;
            set => SetProperty(ref skipCollapsedFolders, value);
        }
    }
}
