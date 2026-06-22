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
            AttachNavAnimations();
        }

        private void AttachNavAnimations()
        {
            var navButtons = new[] { Menu1B, Menu2B, Menu3B, Menu4B, Menu5B };
            foreach (var btn in navButtons)
            {
                btn.MouseEnter += NavButton_MouseEnter;
                btn.MouseLeave += NavButton_MouseLeave;
            }
        }

        private void NavButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button btn && btn.Template != null)
            {
                var btnBg = btn.Template.FindName("BtnBg", btn) as Border;
                if (btnBg != null)
                    Animator.ScaleTo(btnBg, 1.0, 1.08, 150);
            }
        }

        private void NavButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button btn && btn.Template != null)
            {
                var btnBg = btn.Template.FindName("BtnBg", btn) as Border;
                if (btnBg != null)
                    Animator.ScaleTo(btnBg, 1.08, 1.0, 150);
            }
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
                    UpdateTabStyles(targetIndex);
                    AnimateDashboardHeader(menuName);

                    newControl.Opacity = 0;
                    newControl.RenderTransform = new TranslateTransform(0, 20);
                    newControl.RenderTransformOrigin = new Point(0.5, 0.5);

                    if (_currentControl != null)
                    {
                        _currentControl.RenderTransform = new TranslateTransform(0, 0);
                        _currentControl.RenderTransformOrigin = new Point(0.5, 0.5);
                    }

                    ContentArea.Children.Clear();
                    ContentArea.Children.Add(newControl);
                    Animator.FadeSlide(newControl, 0, 0, 20, 0, 0, 1);
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
            DashTitle.Text = info.title;
            DashDesc.Text = info.desc;
            DashIcon.Text = info.icon;
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
            var tabs = new[] { Menu1B, Menu2B, Menu3B, Menu4B, Menu5B };
            for (int i = 0; i < tabs.Length; i++)
            {
                var btn = tabs[i];
                if (btn.Template == null) continue;
                var activeBar = btn.Template.FindName("ActiveBar", btn) as Border;
                var btnBg = btn.Template.FindName("BtnBg", btn) as Border;
                var textBlocks = FindVisualChildren<TextBlock>(btn);

                if (i == activeIndex)
                {
                    if (activeBar != null)
                    {
                        activeBar.Opacity = 0;
                        Animator.Fade(activeBar);
                    }
                    if (btnBg != null)
                    {
                        var current = (btnBg.Background as SolidColorBrush)?.Color ?? Colors.Transparent;
                        var target = Color.FromArgb(0x18, 0xCC, 0x00, 0x00);
                        btnBg.Background = new SolidColorBrush(current);
                        Animator.ColorTo(btnBg, "Background.Color", current, target, 300);
                    }
                    foreach (var tb in textBlocks)
                    {
                        var current = (tb.Foreground as SolidColorBrush)?.Color ?? Colors.Gray;
                        var target = ((SolidColorBrush)FindResource("NeonTextPrimary")).Color;
                        tb.Foreground = new SolidColorBrush(current);
                        Animator.ColorTo(tb, "Foreground.Color", current, target, 300);
                    }
                }
                else
                {
                    if (activeBar != null) activeBar.Opacity = 0;
                    if (btnBg != null) btnBg.Background = Brushes.Transparent;
                    foreach (var tb in textBlocks)
                        tb.Foreground = (SolidColorBrush)FindResource("NeonTextMuted");
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

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e) { }

        public void UpdateToggleUI(AToggle toggle, bool state)
        {
            if (state) toggle.EnableSwitch();
            else toggle.DisableSwitch();
        }

        public void ApplyThemeColor(System.Windows.Media.Color color)
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
