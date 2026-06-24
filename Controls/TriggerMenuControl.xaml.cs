using ShadowCheat.Class;
using ShadowCheat.Class.Features;
using ShadowCheat.UILibrary;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ShadowCheat.Controls
{
    public partial class TriggerMenuControl : UserControl
    {
        private MainWindow? _mainWindow;
        private bool _isInitialized;
        private readonly Dictionary<string, bool> _localMinimizeState = new()
        {
            { "Trigger Bot", false }, { "Aim Key Settings", false }, { "Rapid Fire", false }
        };

        public StackPanel TriggerBotPanel_ => TriggerBotPanel;
        public StackPanel AimKeyPanel_ => AimKeyPanel;
        public StackPanel RapidFirePanel_ => RapidFirePanel;

        public TriggerMenuControl() { InitializeComponent(); }

        public void Initialize(MainWindow mainWindow)
        {
            if (_isInitialized) return;
            _mainWindow = mainWindow;
            _isInitialized = true;

            LoadTriggerBot();
            LoadAimKey();
            LoadRapidFire();
            ApplyMinimizeStates();
        }

        private void ApplyMinimizeStates()
        {
            ApplyPanelState("Trigger Bot", TriggerBotPanel);
            ApplyPanelState("Aim Key Settings", AimKeyPanel);
            ApplyPanelState("Rapid Fire", RapidFirePanel);
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
                    if (f != null) f.Enabled = !f.Enabled;
                };
            }, "Auto-fire when crosshair is on target.");
            b.AddSlider("Trigger Radius", "px", 1, 1, 5, 80, s =>
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
            b.AddToggle("Headshot Only", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<TriggerBot>();
                    if (f != null)
                    {
                        f.HeadshotOnly = !f.HeadshotOnly;
                        if (f.HeadshotOnly) t.EnableSwitch(); else t.DisableSwitch();
                    }
                };
            }, "Only fire when crosshair is in the head area (upper zone) of the target.");
            b.AddSlider("Head Zone", "%", 1, 1, 10, 50, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<TriggerBot>();
                    if (f != null) f.HeadshotZonePct = (float)s.Slider.Value;
                };
            }, "Percentage of the target height considered as head zone (top portion).");
            b.AddToggle("Movement Trigger", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<TriggerBot>();
                    if (f != null)
                    {
                        f.RequireMovement = !f.RequireMovement;
                        if (f.RequireMovement) t.EnableSwitch(); else t.DisableSwitch();
                    }
                };
            }, "Only fire if the target is moving (tracks position changes frame-to-frame).");
            b.AddSlider("Confirmation Frames", "f", 1, 1, 1, 10, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<TriggerBot>();
                    if (f != null) f.ConfirmationFrames = (int)s.Slider.Value;
                };
            }, "How many consecutive frames the target must be detected before firing (anti-spoof).");
            b.AddSeparator();
        }

        private void LoadAimKey()
        {
            var b = new SectionBuilder(AimKeyPanel);
            b.AddTitle("Aim Key Settings", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Aim Key Settings", AimKeyPanel);
                t.SetMinimizedIcon(_localMinimizeState["Aim Key Settings"]);
            });
            b.AddToggle("Require Aim Key", t =>
            {
                var triggerBot = _mainWindow?.FeatureManager.GetFeature<TriggerBot>();
                if (triggerBot != null && !triggerBot.RequireAimKey) t.EnableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<TriggerBot>();
                    if (f != null)
                    {
                        f.RequireAimKey = !f.RequireAimKey;
                        if (f.RequireAimKey) t.EnableSwitch(); else t.DisableSwitch();
                    }
                };
            }, "Trigger Bot only fires when aim key (RMB) is held.");
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
                    if (f != null) f.Enabled = !f.Enabled;
                };
            }, "Automatically fire as fast as possible while holding fire button.");
            b.AddSlider("CPS Limit", "cps", 1, 1, 5, 20, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<RapidFire>();
                    if (f != null) f.CpsLimit = (int)s.Slider.Value;
                };
            }, "Maximum clicks per second (humanized).");
            b.AddSeparator();
        }

        private SectionBuilder GetBuilder(StackPanel panel) => new(panel);
    }
}
