using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Piktosaur.ViewModels;

namespace Piktosaur.Views
{
    public sealed partial class SlideshowSettingsButton : UserControl
    {
        public SlideshowSettingsButton()
        {
            InitializeComponent();
        }

        private void OnSettingsFlyoutClosing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
        {
            // Flyout closes naturally when clicking outside
        }

        private void OnAnimationSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AnimationComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                SettingsVM.Shared.SlideshowAnimation = tag switch
                {
                    "Fade" => SlideshowAnimation.Fade,
                    "Slide" => SlideshowAnimation.Slide,
                    "Zoom" => SlideshowAnimation.Zoom,
                    _ => SlideshowAnimation.None
                };
            }
        }

        private void OnRandomOrderToggled(object sender, RoutedEventArgs e)
        {
            SettingsVM.Shared.UseRandomOrder = RandomOrderToggle.IsOn;
        }

        private void OnSkipCollapsedToggled(object sender, RoutedEventArgs e)
        {
            SettingsVM.Shared.SkipCollapsedFolders = SkipCollapsedToggle.IsOn;
        }
    }
}
