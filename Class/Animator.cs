using System.Windows;
using System.Windows.Media.Animation;

namespace ShadowCheat.Class
{
    public static class Animator
    {
        private static readonly TimeSpan duration = TimeSpan.FromMilliseconds(500);

        private static readonly IEasingFunction Smooth = new QuarticEase
        {
            EasingMode = EasingMode.EaseInOut
        };

        public static void Fade(DependencyObject Object)
        {
            var sb = new Storyboard();
            DoubleAnimation FadeIn = new()
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(duration),
            };
            Storyboard.SetTarget(FadeIn, Object);
            Storyboard.SetTargetProperty(FadeIn, new PropertyPath("Opacity"));
            sb.Children.Add(FadeIn);
            sb.Begin();
        }

        public static void FadeOut(DependencyObject Object)
        {
            var sb = new Storyboard();
            DoubleAnimation Fade = new()
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(duration),
            };
            Storyboard.SetTarget(Fade, Object);
            Storyboard.SetTargetProperty(Fade, new PropertyPath("Opacity"));
            sb.Children.Add(Fade);
            sb.Begin();
        }

        public static void ObjectShift(Duration speed, DependencyObject Object, Thickness Get, Thickness Set)
        {
            var sb = new Storyboard();
            ThicknessAnimation Animation = new()
            {
                From = Get,
                To = Set,
                Duration = speed,
                EasingFunction = Smooth,
            };
            Storyboard.SetTarget(Animation, Object);
            Storyboard.SetTargetProperty(Animation, new PropertyPath("(Panel.Margin)"));
            sb.Children.Add(Animation);
            sb.Begin();
        }

        public static void WidthShift(Duration speed, FrameworkElement element, double originalSize, double newSize)
        {
            var animation = new DoubleAnimation
            {
                From = originalSize,
                To = newSize,
                Duration = speed,
                EasingFunction = new QuarticEase()
            };
            element.BeginAnimation(FrameworkElement.WidthProperty, animation);
        }

        public static void HeightShift(Duration speed, FrameworkElement element, double originalSize, double newSize)
        {
            var animation = new DoubleAnimation
            {
                From = originalSize,
                To = newSize,
                Duration = speed,
                EasingFunction = new QuarticEase()
            };
            element.BeginAnimation(FrameworkElement.HeightProperty, animation);
        }
    }
}
