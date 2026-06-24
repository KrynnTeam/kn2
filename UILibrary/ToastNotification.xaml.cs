using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ShadowCheat.UILibrary
{
    public partial class ToastNotification : UserControl
    {
        private static readonly Color EnabledColor = Color.FromRgb(0xCC, 0x00, 0x00);
        private static readonly Color DisabledColor = Color.FromRgb(0x66, 0x33, 0x33);

        public ToastNotification(string featureName, bool enabled)
        {
            InitializeComponent();

            if (enabled)
            {
                IconText.Text = "\uE73E";
                IconBorder.Background = new SolidColorBrush(Color.FromArgb(0x1A, 0xCC, 0x00, 0x00));
                IconText.Foreground = new SolidColorBrush(EnabledColor);
                Root.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x66, 0x00, 0x00));
                StatusText.Text = "ACTIVADO";
                StatusText.Foreground = new SolidColorBrush(EnabledColor);
            }
            else
            {
                IconText.Text = "\uE711";
                IconBorder.Background = new SolidColorBrush(Color.FromArgb(0x1A, 0x66, 0x33, 0x33));
                IconText.Foreground = new SolidColorBrush(DisabledColor);
                Root.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x1A, 0x1A));
                StatusText.Text = "DESACTIVADO";
                StatusText.Foreground = new SolidColorBrush(DisabledColor);
            }

            MessageText.Text = featureName;
        }

        public void ShowAnimation()
        {
            Opacity = 0;
            RenderTransform = new TranslateTransform(0, -60);
            RenderTransformOrigin = new Point(0.5, 0.5);

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut } };
            BeginAnimation(OpacityProperty, fadeIn);

            var slideIn = new DoubleAnimation(-60, 0, TimeSpan.FromMilliseconds(300))
            { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut } };
            ((TranslateTransform)RenderTransform).BeginAnimation(TranslateTransform.YProperty, slideIn);
        }

        public void Dismiss()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250))
            { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseIn } };
            fadeOut.Completed += (_, _) =>
            {
                var parent = VisualParent as Panel;
                parent?.Children.Remove(this);
            };
            BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
