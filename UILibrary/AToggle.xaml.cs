using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ShadowCheat.UILibrary
{
    public partial class AToggle : System.Windows.Controls.UserControl
    {
        private static readonly Color DisableColor = Color.FromRgb(0x55, 0x4C, 0x70);
        private static readonly Color DisableTrackColor = Color.FromRgb(0x1A, 0x1A, 0x2E);
        private static readonly Color EnableColor = Color.FromRgb(0x00, 0xA8, 0xFF);
        private static readonly Color EnableTrackColor = Color.FromRgb(0x0A, 0x2A, 0x4E);
        private static readonly TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(200);
        private bool _isEnabled = false;

        public AToggle(string Text, string? tooltip = null)
        {
            InitializeComponent();
            ToggleTitle.Content = Text;
            if (!string.IsNullOrEmpty(tooltip))
                ToolTip = new System.Windows.Controls.ToolTip { Content = tooltip };
        }

        public void EnableSwitch()
        {
            _isEnabled = true;
            SwitchBorder.BorderBrush = new SolidColorBrush(EnableColor);
            SwitchBorder.Background = new SolidColorBrush(EnableTrackColor);
            SetColorAnimation(DisableColor, EnableColor, AnimationDuration);
            AnimObjectShift(SwitchMoving, new Thickness(2, 0, 0, 0), new Thickness(0, 0, 2, 0));
        }

        public void DisableSwitch()
        {
            _isEnabled = false;
            SwitchBorder.BorderBrush = new SolidColorBrush(DisableColor);
            SwitchBorder.Background = new SolidColorBrush(DisableTrackColor);
            SetColorAnimation(EnableColor, DisableColor, AnimationDuration);
            AnimObjectShift(SwitchMoving, new Thickness(0, 0, 2, 0), new Thickness(2, 0, 0, 0));
        }

        private void SetColorAnimation(Color fromColor, Color toColor, TimeSpan duration)
        {
            ColorAnimation animation = new ColorAnimation(fromColor, toColor, duration);
            SwitchMoving.Background = new SolidColorBrush(fromColor);
            SwitchMoving.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        private void AnimObjectShift(FrameworkElement element, Thickness from, Thickness to)
        {
            ThicknessAnimation anim = new()
            {
                From = from,
                To = to,
                Duration = AnimationDuration,
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(anim, element);
            Storyboard.SetTargetProperty(anim, new PropertyPath("Margin"));
            Storyboard sb = new();
            sb.Children.Add(anim);
            sb.Begin();
        }
    }
}
