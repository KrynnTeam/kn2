using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace ShadowCheat.Class
{
    public static class Animator
    {
        private static readonly TimeSpan duration = TimeSpan.FromMilliseconds(400);
        private static readonly IEasingFunction SmoothInOut = new QuarticEase { EasingMode = EasingMode.EaseInOut };
        private static readonly IEasingFunction SmoothOut = new QuarticEase { EasingMode = EasingMode.EaseOut };
        private static readonly IEasingFunction SmoothIn = new QuarticEase { EasingMode = EasingMode.EaseIn };

        public static void Fade(DependencyObject obj)
        {
            var sb = new Storyboard();
            var anim = new DoubleAnimation { From = 0.0, To = 1.0, Duration = new Duration(duration), EasingFunction = SmoothOut };
            Storyboard.SetTarget(anim, obj);
            Storyboard.SetTargetProperty(anim, new PropertyPath("Opacity"));
            sb.Children.Add(anim);
            sb.Begin();
        }

        public static void FadeOut(DependencyObject obj)
        {
            var sb = new Storyboard();
            var anim = new DoubleAnimation { From = 1.0, To = 0.0, Duration = new Duration(duration), EasingFunction = SmoothIn };
            Storyboard.SetTarget(anim, obj);
            Storyboard.SetTargetProperty(anim, new PropertyPath("Opacity"));
            sb.Children.Add(anim);
            sb.Begin();
        }

        public static void FadeTo(DependencyObject obj, double from, double to)
        {
            var sb = new Storyboard();
            var anim = new DoubleAnimation { From = from, To = to, Duration = new Duration(duration), EasingFunction = SmoothInOut };
            Storyboard.SetTarget(anim, obj);
            Storyboard.SetTargetProperty(anim, new PropertyPath("Opacity"));
            sb.Children.Add(anim);
            sb.Begin();
        }

        public static void FadeSlide(DependencyObject obj, double fromX, double toX, double fromY, double toY, double fromOpacity, double toOpacity)
        {
            var sb = new Storyboard();
            var translateX = new DoubleAnimation { From = fromX, To = toX, Duration = new Duration(duration), EasingFunction = SmoothOut };
            Storyboard.SetTarget(translateX, obj);
            Storyboard.SetTargetProperty(translateX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            sb.Children.Add(translateX);
            var translateY = new DoubleAnimation { From = fromY, To = toY, Duration = new Duration(duration), EasingFunction = SmoothOut };
            Storyboard.SetTarget(translateY, obj);
            Storyboard.SetTargetProperty(translateY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            sb.Children.Add(translateY);
            var fade = new DoubleAnimation { From = fromOpacity, To = toOpacity, Duration = new Duration(duration), EasingFunction = SmoothOut };
            Storyboard.SetTarget(fade, obj);
            Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
            sb.Children.Add(fade);
            sb.Begin();
        }

        public static void ScaleTo(FrameworkElement element, double from, double to, double ms = 200)
        {
            var d = TimeSpan.FromMilliseconds(ms);
            var sx = new DoubleAnimation { From = from, To = to, Duration = new Duration(d), EasingFunction = SmoothOut };
            var sy = new DoubleAnimation { From = from, To = to, Duration = new Duration(d), EasingFunction = SmoothOut };
            element.RenderTransformOrigin = new Point(0.5, 0.5);
            element.RenderTransform = new ScaleTransform(from, from);
            element.BeginAnimation(ScaleTransform.ScaleXProperty, sx);
            element.BeginAnimation(ScaleTransform.ScaleYProperty, sy);
        }

        public static void PulseOpacity(FrameworkElement element, double min, double max, double ms)
        {
            var anim = new DoubleAnimation { From = min, To = max, Duration = TimeSpan.FromMilliseconds(ms), AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, EasingFunction = SmoothInOut };
            element.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        public static void ObjectShift(Duration speed, DependencyObject Object, Thickness Get, Thickness Set)
        {
            var sb = new Storyboard();
            var Animation = new ThicknessAnimation { From = Get, To = Set, Duration = speed, EasingFunction = SmoothInOut };
            Storyboard.SetTarget(Animation, Object);
            Storyboard.SetTargetProperty(Animation, new PropertyPath("(Panel.Margin)"));
            sb.Children.Add(Animation);
            sb.Begin();
        }

        public static void WidthShift(Duration speed, FrameworkElement element, double originalSize, double newSize)
        {
            var animation = new DoubleAnimation { From = originalSize, To = newSize, Duration = speed, EasingFunction = SmoothInOut };
            element.BeginAnimation(FrameworkElement.WidthProperty, animation);
        }

        public static void HeightShift(Duration speed, FrameworkElement element, double originalSize, double newSize)
        {
            var animation = new DoubleAnimation { From = originalSize, To = newSize, Duration = speed, EasingFunction = SmoothInOut };
            element.BeginAnimation(FrameworkElement.HeightProperty, animation);
        }

        public static void ColorTo(DependencyObject target, string property, Color from, Color to, double ms = 250)
        {
            if (from == to) return;
            var anim = new ColorAnimation { From = from, To = to, Duration = TimeSpan.FromMilliseconds(ms), EasingFunction = SmoothInOut };
            Storyboard.SetTarget(anim, target);
            Storyboard.SetTargetProperty(anim, new PropertyPath(property));
            var sb = new Storyboard();
            sb.Children.Add(anim);
            sb.Begin();
        }
    }
}
