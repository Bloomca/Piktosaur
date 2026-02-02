using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Piktosaur.ViewModels;

namespace Piktosaur.Views
{
    public sealed partial class SlideshowImageView : UserControl
    {
        private static readonly TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(300);

        private bool useImageA = true;
        private bool isFirstImage = true;

        public SlideshowImageView()
        {
            InitializeComponent();
        }

        public void SetImage(string? path)
        {
            if (path == null) return;

            var bitmap = new BitmapImage();
            bitmap.UriSource = new Uri(path);

            var incomingImage = useImageA ? ImageA : ImageB;
            var outgoingImage = useImageA ? ImageB : ImageA;
            var incomingTransform = useImageA ? TransformA : TransformB;
            var outgoingTransform = useImageA ? TransformB : TransformA;

            incomingImage.Source = bitmap;

            var animation = SettingsVM.Shared.SlideshowAnimation;

            if (isFirstImage || animation == SlideshowAnimation.None)
            {
                incomingImage.Opacity = 1;
                outgoingImage.Opacity = 0;
                ResetTransform(incomingTransform);
                ResetTransform(outgoingTransform);
                isFirstImage = false;
            }
            else
            {
                switch (animation)
                {
                    case SlideshowAnimation.Fade:
                        AnimateFade(incomingImage, outgoingImage);
                        break;
                    case SlideshowAnimation.Zoom:
                        AnimateZoom(incomingImage, outgoingImage, incomingTransform, outgoingTransform);
                        break;
                }
            }

            useImageA = !useImageA;
        }

        private void ResetTransform(CompositeTransform transform)
        {
            transform.TranslateX = 0;
            transform.ScaleX = 1;
            transform.ScaleY = 1;
        }

        private void AnimateFade(Image incoming, Image outgoing)
        {
            var storyboard = new Storyboard();

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(AnimationDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fadeIn, incoming);
            Storyboard.SetTargetProperty(fadeIn, "Opacity");

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(AnimationDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fadeOut, outgoing);
            Storyboard.SetTargetProperty(fadeOut, "Opacity");

            storyboard.Children.Add(fadeIn);
            storyboard.Children.Add(fadeOut);
            storyboard.Begin();
        }

        private void AnimateZoom(Image incoming, Image outgoing,
            CompositeTransform inTransform, CompositeTransform outTransform)
        {
            var storyboard = new Storyboard();

            incoming.Opacity = 0;
            inTransform.ScaleX = 0.8;
            inTransform.ScaleY = 0.8;

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(AnimationDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fadeIn, incoming);
            Storyboard.SetTargetProperty(fadeIn, "Opacity");

            var scaleInX = new DoubleAnimation
            {
                From = 0.8,
                To = 1,
                Duration = new Duration(AnimationDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleInX, inTransform);
            Storyboard.SetTargetProperty(scaleInX, "ScaleX");

            var scaleInY = new DoubleAnimation
            {
                From = 0.8,
                To = 1,
                Duration = new Duration(AnimationDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleInY, inTransform);
            Storyboard.SetTargetProperty(scaleInY, "ScaleY");

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(AnimationDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fadeOut, outgoing);
            Storyboard.SetTargetProperty(fadeOut, "Opacity");

            var scaleOutX = new DoubleAnimation
            {
                From = 1,
                To = 1.2,
                Duration = new Duration(AnimationDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleOutX, outTransform);
            Storyboard.SetTargetProperty(scaleOutX, "ScaleX");

            var scaleOutY = new DoubleAnimation
            {
                From = 1,
                To = 1.2,
                Duration = new Duration(AnimationDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleOutY, outTransform);
            Storyboard.SetTargetProperty(scaleOutY, "ScaleY");

            storyboard.Children.Add(fadeIn);
            storyboard.Children.Add(scaleInX);
            storyboard.Children.Add(scaleInY);
            storyboard.Children.Add(fadeOut);
            storyboard.Children.Add(scaleOutX);
            storyboard.Children.Add(scaleOutY);

            storyboard.Completed += (s, e) =>
            {
                ResetTransform(outTransform);
            };
            storyboard.Begin();
        }
    }
}
