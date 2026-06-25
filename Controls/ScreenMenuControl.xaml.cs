using ShadowCheat.Class;
using ShadowCheat.Class.Features;
using ShadowCheat.UILibrary;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace ShadowCheat.Controls
{
    public partial class ScreenMenuControl : UserControl
    {
        private MainWindow? _mainWindow;
        private bool _isInitialized;
        private readonly Dictionary<string, bool> _localMinimizeState = new()
        {
            { "Trigger Bot", false }, { "Rapid Fire", false }, { "Visual Overlay", false }
        };

        public StackPanel TriggerBotPanel_ => TriggerBotPanel;
        public StackPanel RapidFirePanel_ => RapidFirePanel;
        public StackPanel VisualPanel_ => VisualPanel;

        public ScreenMenuControl() { InitializeComponent(); }

        public void Initialize(MainWindow mainWindow)
        {
            if (_isInitialized) return;
            _mainWindow = mainWindow;
            _isInitialized = true;

            LoadTriggerBot();
            LoadRapidFire();
            LoadVisual();
            ApplyMinimizeStates();
        }

        private void ApplyMinimizeStates()
        {
            ApplyPanelState("Trigger Bot", TriggerBotPanel);
            ApplyPanelState("Rapid Fire", RapidFirePanel);
            ApplyPanelState("Visual Overlay", VisualPanel);
        }

        private void ApplyPanelState(string name, StackPanel panel)
        {
            if (!_localMinimizeState.TryGetValue(name, out bool minimized)) return;
            var wrapper = GetSectionWrapper(panel, name);
            if (wrapper == null) return;
            wrapper.BeginAnimation(FrameworkElement.HeightProperty, null);
            wrapper.Height = minimized ? 0 : double.NaN;
            wrapper.Visibility = minimized ? Visibility.Collapsed : Visibility.Visible;
        }

        private void TogglePanel(string name, StackPanel panel)
        {
            if (!_localMinimizeState.ContainsKey(name)) return;
            _localMinimizeState[name] = !_localMinimizeState[name];
            var wrapper = GetSectionWrapper(panel, name);
            if (wrapper == null) return;
            if (_localMinimizeState[name])
                CollapseSection(wrapper);
            else
                ExpandSection(wrapper);
        }

        private static Border? GetSectionWrapper(StackPanel panel, string name)
        {
            for (int i = 0; i < panel.Children.Count; i++)
            {
                if (panel.Children[i] is ATitle t &&
                    string.Equals(t.LabelTitle.Content?.ToString(), name, StringComparison.Ordinal) &&
                    i + 1 < panel.Children.Count &&
                    panel.Children[i + 1] is Border wrapper)
                    return wrapper;
            }
            return null;
        }

        private static void CollapseSection(Border wrapper)
        {
            double h = wrapper.ActualHeight;
            if (h <= 0) return;
            wrapper.Height = h;
            var anim = new DoubleAnimation(h, 0, TimeSpan.FromSeconds(0.3));
            anim.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn };
            anim.Completed += (_, _) =>
            {
                wrapper.BeginAnimation(FrameworkElement.HeightProperty, null);
                wrapper.Height = double.NaN;
                wrapper.Visibility = Visibility.Collapsed;
            };
            wrapper.BeginAnimation(FrameworkElement.HeightProperty, anim);
        }

        private static void ExpandSection(Border wrapper)
        {
            wrapper.BeginAnimation(FrameworkElement.HeightProperty, null);
            wrapper.Visibility = Visibility.Visible;
            wrapper.Height = double.NaN;
            wrapper.UpdateLayout();
            double target = wrapper.ActualHeight;
            if (target <= 0) return;
            wrapper.Height = 0;
            wrapper.UpdateLayout();
            var anim = new DoubleAnimation(0, target, TimeSpan.FromSeconds(0.3));
            anim.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            anim.Completed += (_, _) =>
            {
                wrapper.BeginAnimation(FrameworkElement.HeightProperty, null);
                wrapper.Height = double.NaN;
            };
            wrapper.BeginAnimation(FrameworkElement.HeightProperty, anim);
        }

        private void LoadTriggerBot()
        {
            var b = new SectionBuilder(TriggerBotPanel);
            b.AddTitle("Trigger Bot", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Trigger Bot", TriggerBotPanel);
                t.SetMinimizedIcon(_localMinimizeState["Trigger Bot"]);
            });
            b.AddToggle("Auto Trigger", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<TriggerBot>();
                    if (f != null) { f.Enabled = !f.Enabled; SetSwitch(t, f.Enabled); }
                };
            }, "Auto-fire when crosshair is on target.");
            b.AddSlider("Trigger Radius", "px", 5, 1, 10, 250, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<TriggerBot>();
                    if (f != null) f.TriggerRadius = (float)s.Slider.Value;
                };
            }, "Detection radius around crosshair.");
            b.AddSlider("Trigger Delay", "ms", 1, 1, 0, 50, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<TriggerBot>();
                    if (f != null) f.DelayMs = (int)s.Slider.Value;
                };
            }, "Randomized delay before firing (human-like).");
            b.AddToggle("Require Aim Key", t =>
            {
                t.DisableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<TriggerBot>();
                    if (f != null) { f.RequireAimKey = !f.RequireAimKey; SetSwitch(t, f.RequireAimKey); }
                };
            }, "Trigger only fires when aim key (RMB) is held.");
            b.AddToggle("Headshot Only", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<TriggerBot>();
                    if (f != null) { f.HeadshotOnly = !f.HeadshotOnly; SetSwitch(t, f.HeadshotOnly); }
                };
            }, "Only fire when crosshair is in the head area of the target.");
            b.AddSlider("Head Zone", "%", 1, 1, 10, 60, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<TriggerBot>();
                    if (f != null) f.HeadshotZonePct = (float)s.Slider.Value;
                };
            }, "Percentage of target height considered as head zone.");
            b.AddToggle("Movement Trigger", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<TriggerBot>();
                    if (f != null) { f.RequireMovement = !f.RequireMovement; SetSwitch(t, f.RequireMovement); }
                };
            }, "Only fire if the target is moving between frames.");
            b.AddSlider("Confirmation Frames", "f", 1, 1, 1, 10, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<TriggerBot>();
                    if (f != null) f.ConfirmationFrames = (int)s.Slider.Value;
                };
            }, "Consecutive detections required before firing (anti-spoof).");
            b.AddSeparator();
        }

        private void LoadRapidFire()
        {
            var b = new SectionBuilder(RapidFirePanel);
            b.AddTitle("Rapid Fire", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Rapid Fire", RapidFirePanel);
                t.SetMinimizedIcon(_localMinimizeState["Rapid Fire"]);
            });
            b.AddToggle("Rapid Fire", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<RapidFire>();
                    if (f != null) { f.Enabled = !f.Enabled; SetSwitch(t, f.Enabled); }
                };
            }, "Auto-fire while holding fire button.");
            b.AddSlider("CPS Limit", "cps", 1, 1, 5, 20, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<RapidFire>();
                    if (f != null) f.CpsLimit = (int)s.Slider.Value;
                };
            }, "Maximum clicks per second.");
            b.AddSeparator();
        }

        private void LoadVisual()
        {
            var b = new SectionBuilder(VisualPanel);
            b.AddTitle("Visual Overlay", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Visual Overlay", VisualPanel);
                t.SetMinimizedIcon(_localMinimizeState["Visual Overlay"]);
            });
            b.AddToggle("Show Overlay", t =>
            {
                t.EnableSwitch();
                t.Reader.Click += (_, _) => _mainWindow?.ToggleOverlayVisibility();
            }, "Show or hide the overlay window.");
            b.AddToggle("Target Box", t =>
            {
                t.EnableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    var overlay = _mainWindow?.GetOverlay();
                    if (overlay != null) { overlay.ShowTargetBox = !overlay.ShowTargetBox; SetSwitch(t, overlay.ShowTargetBox); }
                };
            }, "Draw a box around the detected target.");
            b.AddToggle("FOV Circle", t =>
            {
                t.EnableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    var overlay = _mainWindow?.GetOverlay();
                    if (overlay != null) { overlay.ShowFov = !overlay.ShowFov; SetSwitch(t, overlay.ShowFov); }
                };
            }, "Show scan radius circle.");
            b.AddToggle("Crosshair Lines", t =>
            {
                t.EnableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    var overlay = _mainWindow?.GetOverlay();
                    if (overlay != null) { overlay.ShowCrosshairLines = !overlay.ShowCrosshairLines; SetSwitch(t, overlay.ShowCrosshairLines); }
                };
            }, "Show crosshair lines on the overlay.");
            b.AddSlider("Scan Radius", "px", 1, 1, 50, 400, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var detector = _mainWindow?.FeatureManager.Detector;
                    if (detector != null) detector.Profile.ScanRadius = (int)s.Slider.Value;
                };
            }, "Radius of the scan area around crosshair.");
            b.AddSlider("Min Confidence", "%", 1, 1, 10, 95, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var detector = _mainWindow?.FeatureManager.Detector;
                    if (detector != null) detector.Profile.MinConfidence = (float)(s.Slider.Value / 100.0);
                };
            }, "Minimum color match confidence.");
            b.AddSlider("Min Contrast", "", 1, 1, 1, 50, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var detector = _mainWindow?.FeatureManager.Detector;
                    if (detector != null) detector.Profile.MinContrast = (float)s.Slider.Value;
                };
            }, "Minimum contrast between target and background.");
            b.AddToggle("Topmost", t =>
            {
                t.EnableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    var overlay = _mainWindow?.GetOverlay();
                    if (overlay != null) { overlay.Topmost = !overlay.Topmost; SetSwitch(t, overlay.Topmost); }
                };
            }, "Keep overlay on top of all windows.");
            b.AddToggle("Click-Through", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var overlay = _mainWindow?.GetOverlay();
                    if (overlay != null) { overlay.IsHitTestVisible = !overlay.IsHitTestVisible; SetSwitch(t, !overlay.IsHitTestVisible); }
                };
            }, "Pass clicks through the overlay.");
            b.AddSeparator();
        }

        private static void SetSwitch(AToggle t, bool on)
        {
            if (on) t.EnableSwitch(); else t.DisableSwitch();
        }
    }
}
