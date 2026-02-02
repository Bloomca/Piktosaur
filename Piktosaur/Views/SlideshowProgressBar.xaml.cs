using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Piktosaur.ViewModels;

namespace Piktosaur.Views
{
    public sealed partial class SlideshowProgressBar : UserControl
    {
        private Storyboard? storyboard;

        public SlideshowProgressBar()
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;
            CreateAnimation();
        }

        public void Start()
        {
            storyboard?.Begin();
        }

        public void Pause()
        {
            storyboard?.Pause();
        }

        public void Resume()
        {
            storyboard?.Resume();
        }

        public void Reset()
        {
            storyboard?.Stop();
            ProgressScale.ScaleX = 0;
        }

        public void Restart()
        {
            Reset();
            Start();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ProgressFill.Width = e.NewSize.Width;
        }

        private void CreateAnimation()
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(SlideshowVM.SlideshowInterval)
            };

            Storyboard.SetTarget(animation, ProgressScale);
            Storyboard.SetTargetProperty(animation, "ScaleX");

            storyboard = new Storyboard();
            storyboard.Children.Add(animation);
        }
    }
}
