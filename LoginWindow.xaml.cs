using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace ShadowCheat
{
    public partial class LoginWindow : Window
    {
        private bool _isLogin = true;
        private readonly TranslateTransform _loginSlide = new();
        private readonly TranslateTransform _registerSlide = new();

        public LoginWindow()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                LoginPanel.RenderTransform = _loginSlide;
                RegisterPanel.RenderTransform = _registerSlide;
                _registerSlide.X = 40;
                RegisterPanel.Opacity = 0;
                LoginUsername.Focus();
                StartAnimations();
            };
        }

        private void StartAnimations()
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseInOut };
            var forever = RepeatBehavior.Forever;

            void AnimateFloat(Animatable target, string prop, double from, double to, double sec, double delay = 0)
            {
                target.BeginAnimation(
                    (DependencyProperty)typeof(TranslateTransform).GetField(prop, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!.GetValue(null)!,
                    new DoubleAnimation { From = from, To = to, Duration = TimeSpan.FromSeconds(sec), AutoReverse = true, RepeatBehavior = forever, EasingFunction = ease, BeginTime = TimeSpan.FromSeconds(delay) });
            }

            void AnimateOpacity(UIElement target, double from, double to, double sec, double delay = 0)
            {
                target.BeginAnimation(UIElement.OpacityProperty,
                    new DoubleAnimation { From = from, To = to, Duration = TimeSpan.FromSeconds(sec), AutoReverse = true, RepeatBehavior = forever, EasingFunction = ease, BeginTime = TimeSpan.FromSeconds(delay) });
            }

            AnimateFloat(Orb1Transform, "XProperty", -15, 15, 5);
            AnimateFloat(Orb1Transform, "YProperty", 0, 20, 7);
            AnimateOpacity(Orb1, 1, 0.7, 4);

            AnimateFloat(Orb2Transform, "YProperty", 0, -25, 6);
            AnimateFloat(Orb2Transform, "XProperty", -10, 10, 8);
            AnimateOpacity(Orb2, 1, 0.65, 5);

            AnimateFloat(Orb3Transform, "YProperty", 10, -10, 7);
            AnimateFloat(Orb3Transform, "XProperty", -5, 15, 6);
            AnimateOpacity(Orb3, 1, 0.75, 3.5);

            AnimateFloat(Orb4Transform, "YProperty", -5, 10, 4.5);
            AnimateFloat(Orb4Transform, "XProperty", 0, -10, 5.5);
            AnimateOpacity(Orb4, 1, 0.6, 3);

            LogoGlow.BeginAnimation(DropShadowEffect.OpacityProperty,
                new DoubleAnimation { From = 0.4, To = 0.8, Duration = TimeSpan.FromSeconds(2.5), AutoReverse = true, RepeatBehavior = forever });

            AnimateFloat(Particle1.RenderTransform, "YProperty", 0, -200, 8);
            AnimateOpacity(Particle1, 0.5, 0, 8);
            AnimateFloat(Particle1.RenderTransform, "XProperty", 0, 10, 8);

            AnimateFloat(Particle2.RenderTransform, "YProperty", 0, -160, 10, 1.5);
            AnimateOpacity(Particle2, 0.4, 0, 10, 1.5);
            AnimateFloat(Particle2.RenderTransform, "XProperty", 0, -8, 10, 1.5);

            AnimateFloat(Particle3.RenderTransform, "YProperty", 0, -180, 9, 3);
            AnimateOpacity(Particle3, 0.35, 0, 9, 3);
            AnimateFloat(Particle3.RenderTransform, "XProperty", 0, 12, 9, 3);

            AnimateFloat(Particle4.RenderTransform, "YProperty", 0, -220, 11, 0.8);
            AnimateOpacity(Particle4, 0.3, 0, 11, 0.8);
            AnimateFloat(Particle4.RenderTransform, "XProperty", 0, -6, 11, 0.8);

            AnimateFloat(Particle5.RenderTransform, "YProperty", 0, -260, 12, 2);
            AnimateOpacity(Particle5, 0.45, 0, 12, 2);
            AnimateFloat(Particle5.RenderTransform, "XProperty", 0, -15, 12, 2);

            AnimateFloat(Particle6.RenderTransform, "YProperty", 0, -190, 9, 4);
            AnimateOpacity(Particle6, 0.35, 0, 9, 4);
            AnimateFloat(Particle6.RenderTransform, "XProperty", 0, 8, 9, 4);

            AnimateFloat(Particle7.RenderTransform, "YProperty", 0, -240, 13, 1);
            AnimateOpacity(Particle7, 0.5, 0, 13, 1);
            AnimateFloat(Particle7.RenderTransform, "XProperty", 0, -12, 13, 1);

            AnimateFloat(Particle8.RenderTransform, "YProperty", 0, -170, 10, 5);
            AnimateOpacity(Particle8, 0.4, 0, 10, 5);
            AnimateFloat(Particle8.RenderTransform, "XProperty", 0, 6, 10, 5);

            ShimmerStop1.BeginAnimation(GradientStop.OffsetProperty,
                new DoubleAnimation { From = -0.2, To = 1.2, Duration = TimeSpan.FromSeconds(5), AutoReverse = true, RepeatBehavior = forever, EasingFunction = ease });

            var colorEase = new CubicEase { EasingMode = EasingMode.EaseInOut };
            GradStop1.BeginAnimation(GradientStop.ColorProperty,
                new ColorAnimation { From = Color.FromArgb(0xFF, 0xCC, 0x00, 0x00), To = Color.FromArgb(0xFF, 0x8B, 0x00, 0x00), Duration = TimeSpan.FromSeconds(4), AutoReverse = true, RepeatBehavior = forever, EasingFunction = colorEase });
            GradStop3.BeginAnimation(GradientStop.ColorProperty,
                new ColorAnimation { From = Color.FromArgb(0xFF, 0x8B, 0x00, 0x00), To = Color.FromArgb(0xFF, 0xCC, 0x00, 0x00), Duration = TimeSpan.FromSeconds(4), AutoReverse = true, RepeatBehavior = forever, EasingFunction = colorEase });
        }

        private void LoginTab_Click(object sender, MouseButtonEventArgs e) => ShowLogin();
        private void RegisterTab_Click(object sender, MouseButtonEventArgs e) => ShowRegister();

        private void ShowLogin()
        {
            if (_isLogin) return;
            _isLogin = true;

            LoginTab.Background = new SolidColorBrush(Color.FromArgb(0x22, 0xCC, 0x00, 0x00));
            ((TextBlock)((Border)LoginTab).Child).Foreground = Brushes.White;
            RegisterTab.Background = Brushes.Transparent;
            ((TextBlock)((Border)RegisterTab).Child).Foreground = (Brush)new BrushConverter().ConvertFrom("#55FFFFFF")!;

            LoginPanel.IsHitTestVisible = true;
            RegisterPanel.IsHitTestVisible = false;
            AnimatePanel(_loginSlide, 0, LoginPanel, 1);
            AnimatePanel(_registerSlide, 40, RegisterPanel, 0);

            LoginError.Visibility = Visibility.Collapsed;
            LoginUsername.Focus();
            ForgotLink.Visibility = Visibility.Visible;
        }

        private void ShowRegister()
        {
            if (!_isLogin) return;
            _isLogin = false;

            RegisterTab.Background = new SolidColorBrush(Color.FromArgb(0x22, 0xCC, 0x00, 0x00));
            ((TextBlock)((Border)RegisterTab).Child).Foreground = Brushes.White;
            LoginTab.Background = Brushes.Transparent;
            ((TextBlock)((Border)LoginTab).Child).Foreground = (Brush)new BrushConverter().ConvertFrom("#55FFFFFF")!;

            RegisterPanel.IsHitTestVisible = true;
            LoginPanel.IsHitTestVisible = false;
            AnimatePanel(_registerSlide, 0, RegisterPanel, 1);
            AnimatePanel(_loginSlide, -40, LoginPanel, 0);

            RegisterError.Visibility = Visibility.Collapsed;
            RegisterSuccess.Visibility = Visibility.Collapsed;
            RegUsername.Focus();
            ForgotLink.Visibility = Visibility.Collapsed;
        }

        private void AnimatePanel(TranslateTransform slide, double targetX, Border panel, double targetOpacity)
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            slide.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation { To = targetX, Duration = TimeSpan.FromSeconds(0.35), EasingFunction = ease });
            panel.BeginAnimation(OpacityProperty, new DoubleAnimation { To = targetOpacity, Duration = TimeSpan.FromSeconds(0.3), EasingFunction = ease });
        }

        private void LoginSubmit_Click(object sender, RoutedEventArgs e) => TryLogin();
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_isLogin) TryLogin();
                else TryRegister();
            }
        }

        private void TryLogin()
        {
            LoginError.Visibility = Visibility.Collapsed;
            string username = LoginUsername.Text.Trim();
            string password = LoginPassword.Password;

            if (username == "" && password == "")
            {
                LoginError.Text = "   Please enter credentials.";
                LoginError.Visibility = Visibility.Visible;
                return;
            }

            if (username == "admin" && password == "admin")
            {
                var main = new MainWindow();
                main.Show();
                Close();
            }
            else
            {
                LoginError.Visibility = Visibility.Visible;
                LoginPassword.Password = "";
                LoginPassword.Focus();
            }
        }

        private void RegisterSubmit_Click(object sender, RoutedEventArgs e) => TryRegister();

        private void TryRegister()
        {
            RegisterError.Visibility = Visibility.Collapsed;
            RegisterSuccess.Visibility = Visibility.Collapsed;

            string username = RegUsername.Text.Trim();
            string password = RegPassword.Password;
            string confirm = RegConfirm.Password;

            if (username == "" || password == "" || confirm == "")
            {
                RegisterError.Visibility = Visibility.Visible;
                return;
            }

            if (password != confirm)
            {
                RegisterError.Text = "   Passwords do not match.";
                RegisterError.Visibility = Visibility.Visible;
                RegPassword.Password = "";
                RegConfirm.Password = "";
                RegPassword.Focus();
                return;
            }

            if (password.Length < 4)
            {
                RegisterError.Text = "   Password too short (min 4).";
                RegisterError.Visibility = Visibility.Visible;
                RegPassword.Password = "";
                RegConfirm.Password = "";
                RegPassword.Focus();
                return;
            }

            RegisterSuccess.Visibility = Visibility.Visible;
            RegUsername.Text = "";
            RegPassword.Password = "";
            RegConfirm.Password = "";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}