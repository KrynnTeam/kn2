using ShadowCheat.Class;
using ShadowCheat.Class.Features;
using ShadowCheat.Controls;
using ShadowCheat.UILibrary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ShadowCheat
{
    public partial class MainWindow : Window
    {
        private readonly Lazy<UI> _uiManager = new(() => new UI());
        public UI uiManager => _uiManager.Value;
        internal Dictionary<string, AToggle> toggleInstances = new();

        private readonly Dictionary<string, UserControl?> _menuControls = new();
        private readonly Dictionary<string, bool> _menuInitialized = new();
        private UserControl? _currentControl;
        private string _currentMenu = "HomePage";
        private bool _currentlySwitching;
        private readonly Button[] _navTabs = new Button[5];
        private TextBlock?[] _tabTextBlocks = new TextBlock?[5];

        private System.Windows.Forms.NotifyIcon? _trayIcon;

        private static readonly string[] MenuNames = { "HomePage", "ModelMenu", "AimMenu", "SettingsMenu", "AcercaDe" };

        private static readonly Dictionary<string, (string title, string desc, string icon)> DashboardInfo = new()
        {
            ["HomePage"] = ("Inicio", "Bienvenido a KN2", "\uE80F"),
            ["ModelMenu"] = ("Juegos", "Selecciona tu juego y configura el modelo", "\uE7AC"),
            ["AimMenu"] = ("Cheats", "Opciones de asistencia y rendimiento", "\uE768"),
            ["SettingsMenu"] = ("Ajustes", "Configuración general de la aplicación", "\uE713"),
            ["AcercaDe"] = ("Acerca de", "Información del proyecto y créditos", "\uE946"),
        };

        private FeatureManager _featureManager = new();
        private HwidSpoofing? _hwidSpoofer;
        private OverlayWindow? _overlay;
        private Thread? _overlayThread;

        public MainWindow() { InitializeComponent(); }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMainClip();
            LoadInitialMenu();
            StartGradientAnimation();
            InitializeFeatures();
            ShowOverlay();
            PlayEntranceAnimation();
            InitTrayIcon();
        }

        private void PlayEntranceAnimation()
        {
            MainBorder.Opacity = 0;
            MainBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            var scale = new ScaleTransform(0.92, 0.92);
            MainBorder.RenderTransform = scale;
            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500)) { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut } };
            MainBorder.BeginAnimation(UIElement.OpacityProperty, fade);
            var grow = new DoubleAnimation(0.92, 1.0, TimeSpan.FromMilliseconds(500)) { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut } };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, grow);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, grow);
        }

        private void MainBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMainClip();
        }

        private void UpdateMainClip()
        {
            double r = 24;
            var child = MainBorder.Child as FrameworkElement;
            if (child != null)
                child.Clip = new RectangleGeometry(
                    new Rect(0, 0, MainBorder.ActualWidth, MainBorder.ActualHeight), r, r);
        }

        public FeatureManager FeatureManager => _featureManager;

        private void ShowOverlay()
        {
            _overlayThread = new Thread(() =>
            {
                _overlay = new OverlayWindow();
                _overlay.Show();
                System.Windows.Threading.Dispatcher.Run();
            });
            _overlayThread.SetApartmentState(ApartmentState.STA);
            _overlayThread.IsBackground = true;
            _overlayThread.Start();

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            timer.Tick += (s, e) => UpdateOverlay();
            timer.Start();
        }

        private void UpdateOverlay()
        {
            if (_overlay == null) return;
            var state = _featureManager.State;
            var detector = _featureManager.Detector;

            _overlay.ShowFov = true;
            _overlay.FovRadius = detector.Profile.ScanRadius;

            if (state.BestTarget != null && state.TargetVisible)
            {
                _overlay.ShowTargetBox = true;
                _overlay.TargetX = state.BestTarget.CenterX;
                _overlay.TargetY = state.BestTarget.CenterY;
                _overlay.TargetW = state.BestTarget.Width;
                _overlay.TargetH = state.BestTarget.Height;
            }
            else
            {
                _overlay.ShowTargetBox = false;
            }

            _overlay.UpdateOverlay();
        }

        private void InitializeFeatures()
        {
            _featureManager.AddFeature<CrosshairPlacementAssist>();
            _featureManager.AddFeature<StandstillAccuracy>();
            _featureManager.AddFeature<ShotOverride>();
            _featureManager.AddFeature<NoRecoilNoise>();
            _featureManager.AddFeature<VisibilityAimLock>();
            _featureManager.AddFeature<FlickAssist>();
            _hwidSpoofer = _featureManager.AddFeature<HwidSpoofing>();
            _hwidSpoofer.AttachToWindow(this);
            _featureManager.Start();
        }

        private void LoadInitialMenu()
        {
            var control = GetOrCreateMenuControl("HomePage");
            InitializeMenuControl("HomePage", control);
            _menuInitialized["HomePage"] = true;
            ContentArea.Children.Clear();
            ContentArea.Children.Add(control);
            _currentControl = control;
            _currentMenu = "HomePage";
            UpdateTabStyles(0);
            AnimateDashboardHeader("HomePage");
        }

        private UserControl GetOrCreateMenuControl(string menuName)
        {
            if (_menuControls.TryGetValue(menuName, out var existing) && existing != null)
                return existing;

            UserControl control = menuName switch
            {
                "HomePage" => new HomePageControl(),
                "ModelMenu" => new JuegosPage(),
                "AimMenu" => new AimMenuControl(),
                "SettingsMenu" => new SettingsMenuControl(),
                "AcercaDe" => new AboutMenuControl(),
                _ => throw new ArgumentException("Unknown menu: " + menuName)
            };

            _menuControls[menuName] = control;
            _menuInitialized[menuName] = false;
            return control;
        }

        private void MenuSwitch(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string menuName && !_currentlySwitching)
            {
                if (menuName == _currentMenu) return;
                _currentlySwitching = true;

                try
                {
                    var newControl = GetOrCreateMenuControl(menuName);
                    int targetIndex = Array.IndexOf(MenuNames, menuName);
                    int currentIndex = Array.IndexOf(MenuNames, _currentMenu);
                    UpdateTabStyles(targetIndex);
                    AnimateDashboardHeader(menuName);

                    double slideDir = targetIndex > currentIndex ? 40 : -40;
                    newControl.Opacity = 0;
                    newControl.RenderTransform = new TranslateTransform(slideDir, 0);
                    newControl.RenderTransformOrigin = new Point(0.5, 0.5);

                    if (_currentControl != null)
                    {
                        _currentControl.RenderTransform = new TranslateTransform(0, 0);
                        _currentControl.RenderTransformOrigin = new Point(0.5, 0.5);
                    }

                    ContentArea.Children.Clear();
                    ContentArea.Children.Add(newControl);
                    Animator.FadeSlide(newControl, slideDir, 0, 0, 0, 0, 1);
                    _currentControl = newControl;
                    _currentMenu = menuName;

                    if (!_menuInitialized[menuName])
                    {
                        InitializeMenuControl(menuName, newControl);
                        _menuInitialized[menuName] = true;
                    }
                }
                finally { _currentlySwitching = false; }
            }
        }

        private void AnimateDashboardHeader(string menuName)
        {
            if (!DashboardInfo.TryGetValue(menuName, out var info)) return;

            void SlideElement(FrameworkElement el)
            {
                el.RenderTransformOrigin = new Point(0, 0.5);
                el.RenderTransform = new TranslateTransform(0, 0);
                var sb = new Storyboard();
                var slide = new DoubleAnimation { From = 12, To = 0, Duration = TimeSpan.FromMilliseconds(350), EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTarget(slide, el);
                Storyboard.SetTargetProperty(slide, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                sb.Children.Add(slide);
                var fade = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(350), EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTarget(fade, el);
                Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
                sb.Children.Add(fade);
                sb.Begin();
            }

            DashTitle.Text = info.title;
            DashDesc.Text = info.desc;
            DashIcon.Text = info.icon;

            SlideElement(DashTitle);
            SlideElement(DashDesc);
            SlideElement(DashIcon);
        }

        private void InitializeMenuControl(string menuName, UserControl control)
        {
            switch (control)
            {
                case HomePageControl home:
                    home.Initialize(this);
                    break;
                case JuegosPage juegos:
                    juegos.Initialize(this);
                    break;
                case AimMenuControl aim:
                    aim.Initialize(this);
                    break;
                case SettingsMenuControl settings:
                    settings.Initialize(this);
                    break;
                case AboutMenuControl about:
                    about.Initialize(this);
                    break;
            }
        }

        private void StartGradientAnimation()
        {
            DoubleAnimation anim = new()
            {
                From = 0, To = 360,
                Duration = TimeSpan.FromSeconds(30),
                RepeatBehavior = RepeatBehavior.Forever
            };
            RotaryGradient.BeginAnimation(RotateTransform.AngleProperty, anim);

            DoubleAnimation hexAnim = new()
            {
                From = 0, To = 360,
                Duration = TimeSpan.FromSeconds(60),
                RepeatBehavior = RepeatBehavior.Forever
            };
            HexGrid.RenderTransform = new RotateTransform();
            ((RotateTransform)HexGrid.RenderTransform).BeginAnimation(RotateTransform.AngleProperty, hexAnim);

            DoubleAnimation logoAnim = new()
            {
                From = 0.3, To = 0.8,
                Duration = TimeSpan.FromSeconds(2),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            LogoGlow.BeginAnimation(DropShadowEffect.OpacityProperty, logoAnim);
        }

        private void UpdateTabStyles(int activeIndex)
        {
            _navTabs[0] = Menu1B; _navTabs[1] = Menu2B; _navTabs[2] = Menu3B; _navTabs[3] = Menu4B; _navTabs[4] = Menu5B;
            var sliderTop = 19 + activeIndex * 62;

            if (SliderBar.Opacity < 0.5)
            {
                SliderBar.Opacity = 0;
                Animator.Fade(SliderBar);
            }

            var currentMargin = SliderBar.Margin;
            var anim = new ThicknessAnimation(
                new Thickness(0, currentMargin.Top, 0, 0),
                new Thickness(0, sliderTop, 0, 0),
                TimeSpan.FromMilliseconds(350))
            { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut } };
            SliderBar.BeginAnimation(FrameworkElement.MarginProperty, anim);

            for (int i = 0; i < _navTabs.Length; i++)
            {
                var btn = _navTabs[i];
                if (btn.Template == null) continue;
                var btnBg = btn.Template.FindName("BtnBg", btn) as Border;

                if (_tabTextBlocks[i] == null)
                {
                    foreach (var tb in FindVisualChildren<TextBlock>(btn))
                    {
                        if (tb.Name == "IconText") { _tabTextBlocks[i] = tb; break; }
                    }
                }
                var tbIcon = _tabTextBlocks[i];

                if (i == activeIndex)
                {
                    if (btnBg != null)
                    {
                        var current = (btnBg.Background as SolidColorBrush)?.Color ?? Colors.Transparent;
                        var target = Color.FromArgb(0x18, 0xCC, 0x00, 0x00);
                        btnBg.Background = new SolidColorBrush(current);
                        Animator.ColorTo(btnBg, "Background.Color", current, target, 350);
                    }
                    if (tbIcon != null)
                    {
                        var current = (tbIcon.Foreground as SolidColorBrush)?.Color ?? Colors.Gray;
                        var target = ((SolidColorBrush)FindResource("NeonTextPrimary")).Color;
                        tbIcon.Foreground = new SolidColorBrush(current);
                        Animator.ColorTo(tbIcon, "Foreground.Color", current, target, 350);
                    }
                }
                else
                {
                    if (btnBg != null) btnBg.Background = Brushes.Transparent;
                    if (tbIcon != null) tbIcon.Foreground = (SolidColorBrush)FindResource("NeonTextMuted");
                }
            }
        }

        private static List<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            var results = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) results.Add(t);
                results.AddRange(FindVisualChildren<T>(child));
            }
            return results;
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Visual v && FindParent<Button>(v) != null) return;
            DragMove();
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T t) return t;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        public void ToggleOverlayVisibility()
        {
            if (_overlay == null) return;
            if (_overlay.IsVisible) _overlay.Hide();
            else _overlay.Show();
        }

        public void SetProcessPriority(int level)
        {
            var p = System.Diagnostics.Process.GetCurrentProcess();
            p.PriorityClass = level switch
            {
                1 => System.Diagnostics.ProcessPriorityClass.Idle,
                2 => System.Diagnostics.ProcessPriorityClass.BelowNormal,
                3 => System.Diagnostics.ProcessPriorityClass.Normal,
                4 => System.Diagnostics.ProcessPriorityClass.AboveNormal,
                5 => System.Diagnostics.ProcessPriorityClass.High,
                _ => System.Diagnostics.ProcessPriorityClass.Normal
            };
        }

        public void SetAutoStart(bool enabled)
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return;
            if (enabled)
                key.SetValue("KN2", System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "");
            else
                key.DeleteValue("KN2", false);
        }

        public void ApplyTheme(bool darkMode)
        {
            var primary = darkMode ? Color.FromRgb(0xF0, 0xEB, 0xF5) : Color.FromRgb(0x1A, 0x1A, 0x2E);
            var muted = darkMode ? Color.FromRgb(0xA0, 0xA0, 0xA0) : Color.FromRgb(0x66, 0x66, 0x66);
            if (TryFindResource("NeonTextPrimary") is System.Windows.Media.SolidColorBrush bp)
                bp.Color = primary;
            if (TryFindResource("NeonTextMuted") is System.Windows.Media.SolidColorBrush bm)
                bm.Color = muted;
        }

        private void InitTrayIcon()
        {
            _trayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath),
                Text = "KN2",
                Visible = true
            };
            _trayIcon.Click += (_, _) =>
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            };
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            if (_trayIcon != null) _trayIcon.Visible = true;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _trayIcon?.Dispose();
            Application.Current.Shutdown();
        }
        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e) { }

        public void UpdateToggleUI(AToggle toggle, bool state)
        {
            if (state) toggle.EnableSwitch();
            else toggle.DisableSwitch();
        }

        public void ShowNotification(string featureName, bool enabled)
        {
            var toast = new ToastNotification(featureName, enabled);
            NotificationLayer.Children.Insert(0, toast);
            toast.ShowAnimation();

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(2500)
            };
            timer.Tick += (s, _) =>
            {
                timer.Stop();
                toast.Dismiss();
            };
            timer.Start();
        }

        public void ApplyThemeColor(Color color)
        {
            GradientThemeStop.Color = color;
        }

        public string AddDropdownItem(ADropdown dropdown, string text)
        {
            dropdown.DropdownBox.Items.Add(text);
            return text;
        }
    }
}
