using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ShadowCheat.UILibrary
{
    public partial class AToggle : System.Windows.Controls.UserControl
    {
        private static readonly Color DisableColor = Color.FromRgb(0x66, 0x33, 0x33);
        private static readonly Color DisableTrackColor = Color.FromRgb(0x1A, 0x0A, 0x0A);
        private static readonly Color EnableColor = Color.FromRgb(0xCC, 0x00, 0x00);
        private static readonly Color EnableTrackColor = Color.FromRgb(0x33, 0x00, 0x00);
        private static readonly TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(300);
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
            SwitchMoving.Background = new SolidColorBrush(EnableColor);
            AnimObjectShift(SwitchMoving, new Thickness(0, 0, 2, 0));
        }

        public void DisableSwitch()
        {
            _isEnabled = false;
            SwitchBorder.BorderBrush = new SolidColorBrush(DisableColor);
            SwitchBorder.Background = new SolidColorBrush(DisableTrackColor);
            SwitchMoving.Background = new SolidColorBrush(DisableColor);
            AnimObjectShift(SwitchMoving, new Thickness(2, 0, 0, 0));
        }

        private void AnimObjectShift(FrameworkElement element, Thickness to)
        {
            ThicknessAnimation anim = new()
            {
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
