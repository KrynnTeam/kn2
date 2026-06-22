using ShadowCheat.Class;
using ShadowCheat.Class.Features;
using ShadowCheat.UILibrary;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ShadowCheat.Controls
{
    public partial class AimMenuControl : UserControl
    {
        private MainWindow? _mainWindow;
        private bool _isInitialized;
        private readonly Dictionary<string, bool> _localMinimizeState = new()
        {
            { "Detection Config", false }, { "Crosshair Placement", false }, { "Standstill Accuracy", false },
            { "Shot Override", false }, { "No-Recoil", false },
            { "Visibility Lock", false }, { "Flick Assist", false }, { "HWID Spoofing", false }
        };

        public StackPanel DetectionConfigPanel => DetectionPanel;
        public StackPanel CrosshairPanel => CrosshairAssistPanel;
        public StackPanel StandstillPanel_ => StandstillPanel;
        public StackPanel ShotOverridePanel_ => ShotOverridePanel;
        public StackPanel NoRecoilPanel_ => NoRecoilPanel;
        public StackPanel VisibilityPanel_ => VisibilityPanel;
        public StackPanel FlickAssistPanel_ => FlickAssistPanel;
        public StackPanel HwidPanel_ => HwidPanel;

        public AimMenuControl() { InitializeComponent(); }

        public void Initialize(MainWindow mainWindow)
        {
            if (_isInitialized) return;
            _mainWindow = mainWindow;
            _isInitialized = true;

            LoadDetectionConfig();
            LoadCrosshairPlacement();
            LoadStandstillAccuracy();
            LoadShotOverride();
            LoadNoRecoil();
            LoadVisibilityLock();
            LoadFlickAssist();
            LoadHwidSpoofing();
            ApplyMinimizeStates();
        }

        private void ApplyMinimizeStates()
        {
            ApplyPanelState("Detection Config", DetectionPanel);
            ApplyPanelState("Crosshair Placement", CrosshairAssistPanel);
            ApplyPanelState("Standstill Accuracy", StandstillPanel);
            ApplyPanelState("Shot Override", ShotOverridePanel);
            ApplyPanelState("No-Recoil", NoRecoilPanel);
            ApplyPanelState("Visibility Lock", VisibilityPanel);
            ApplyPanelState("Flick Assist", FlickAssistPanel);
            ApplyPanelState("HWID Spoofing", HwidPanel);
        }

        private void ApplyPanelState(string name, StackPanel panel)
        {
            if (_localMinimizeState.TryGetValue(name, out bool minimized))
                SetPanelVisibility(panel, !minimized);
        }

        private void SetPanelVisibility(StackPanel panel, bool visible)
        {
            foreach (UIElement child in panel.Children)
            {
                if (child is ATitle || child is ASpacer || child is ARectangleBottom)
                    child.Visibility = Visibility.Visible;
                else
                    child.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void TogglePanel(string name, StackPanel panel)
        {
            if (!_localMinimizeState.ContainsKey(name)) return;
            _localMinimizeState[name] = !_localMinimizeState[name];
            SetPanelVisibility(panel, !_localMinimizeState[name]);
        }

        private void LoadDetectionConfig()
        {
            var b = GetBuilder(DetectionPanel);
            b.AddTitle("Detection Config", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Detection Config", DetectionPanel);
                t.SetMinimizedIcon(_localMinimizeState["Detection Config"]);
            });
            b.AddToggle("Detection Enabled", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var fm = _mainWindow!.FeatureManager;
                    fm.Detector.Profile.MinConfidence = fm.Detector.Profile.MinConfidence > 0 ? 0 : 0.3f;
                };
            }, "Enable screen-based target detection.");
            b.AddSlider("Scan Radius", "px", 10, 10, 30, 400, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    _mainWindow!.FeatureManager.Detector.Profile.ScanRadius = (int)s.Slider.Value;
                };
            }, "Radius around crosshair to scan for targets.");
            b.AddSlider("Scan Speed", "stride", 1, 1, 1, 10, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    _mainWindow!.FeatureManager.Detector.Profile.ScanStride = (int)s.Slider.Value;
                };
            }, "Higher = faster but less precise.");
            b.AddSlider("Color Tolerance", "%", 5, 5, 5, 150, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    _mainWindow!.FeatureManager.Detector.Profile.Tolerance = (byte)s.Slider.Value;
                };
            }, "How close the color must match.");
            b.AddSlider("Min Cluster", "px", 2, 2, 2, 60, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    _mainWindow!.FeatureManager.Detector.Profile.MinClusterSize = (int)s.Slider.Value;
                };
            }, "Minimum target size in pixels.");
            b.AddColorChanger("Enemy Color", c =>
            {
                var profile = _mainWindow!.FeatureManager.Detector.Profile;
                c.ColorChangingBorder.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(profile.TargetR, profile.TargetG, profile.TargetB));
                c.Reader.Click += (_, _) =>
                {
                    var picker = new ColorPicker(
                        ((System.Windows.Media.SolidColorBrush)c.ColorChangingBorder.Background).Color, "Enemy Color");
                    picker.ColorChanged += color =>
                    {
                        c.ColorChangingBorder.Background = new System.Windows.Media.SolidColorBrush(color);
                        _mainWindow!.FeatureManager.Detector.SetColor(color.R, color.G, color.B);
                    };
                    picker.Show();
                };
            });
            b.AddSeparator();
        }

        private void LoadCrosshairPlacement()
        {
            var b = GetBuilder(CrosshairAssistPanel);
            b.AddTitle("Crosshair Placement", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Crosshair Placement", CrosshairAssistPanel);
                t.SetMinimizedIcon(_localMinimizeState["Crosshair Placement"]);
            });
            b.AddToggle("Placement Assist", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<CrosshairPlacementAssist>();
                    if (f != null) f.Enabled = !f.Enabled;
                };
            }, "Gently pull crosshair toward head trajectory. No snap.");
            b.AddSlider("Drag Strength", "Strength", 0.01, 0.01, 0.01, 0.50, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<CrosshairPlacementAssist>();
                    if (f != null) f.DragStrength = (float)s.Slider.Value;
                };
            }, "How strong the micro-drag feels.");
            b.AddSlider("Activation Radius", "px", 1, 0, 10, 200, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<CrosshairPlacementAssist>();
                    if (f != null) f.ActivationRadius = (float)s.Slider.Value;
                };
            }, "Distance from target before assist kicks in.");
            b.AddSeparator();
        }

        private void LoadStandstillAccuracy()
        {
            var b = GetBuilder(StandstillPanel);
            b.AddTitle("Standstill Accuracy", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Standstill Accuracy", StandstillPanel);
                t.SetMinimizedIcon(_localMinimizeState["Standstill Accuracy"]);
            });
            b.AddToggle("Faster Recovery", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<StandstillAccuracy>();
                    if (f != null) f.Enabled = !f.Enabled;
                };
            }, "Recover accuracy 50-80ms faster after stopping.");
            b.AddSlider("Recovery Boost", "ms", 1, 0, 10, 150, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<StandstillAccuracy>();
                    if (f != null) f.RecoveryBoostMs = (int)s.Slider.Value;
                };
            }, "Extra milliseconds of recovery applied.");
            b.AddSlider("Accuracy Mult", "x", 0.01, 0, 0.3, 1.0, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<StandstillAccuracy>();
                    if (f != null) f.AccuracyMultiplier = (float)s.Slider.Value;
                };
            }, "How much spread is reduced during recovery.");
            b.AddSeparator();
        }

        private void LoadShotOverride()
        {
            var b = GetBuilder(ShotOverridePanel);
            b.AddTitle("Shot Override", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Shot Override", ShotOverridePanel);
                t.SetMinimizedIcon(_localMinimizeState["Shot Override"]);
            });
            b.AddToggle("Peek Trigger", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<ShotOverride>();
                    if (f != null) f.Enabled = !f.Enabled;
                };
            }, "Auto-fire when enemy peeks within trigger radius.");
            b.AddSlider("Trigger Radius", "px", 1, 0, 5, 80, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<ShotOverride>();
                    if (f != null) f.TriggerRadius = (float)s.Slider.Value;
                };
            }, "Radius to auto-fire around crosshair.");
            b.AddSlider("Trigger Delay", "ms", 1, 0, 0, 50, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<ShotOverride>();
                    if (f != null) f.TriggerDelayMs = (int)s.Slider.Value;
                };
            }, "Delay before firing (adds human-like latency).");
            b.AddSeparator();
        }

        private void LoadNoRecoil()
        {
            var b = GetBuilder(NoRecoilPanel);
            b.AddTitle("No-Recoil", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("No-Recoil", NoRecoilPanel);
                t.SetMinimizedIcon(_localMinimizeState["No-Recoil"]);
            });
            b.AddToggle("Noise Recoil Control", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<NoRecoilNoise>();
                    if (f != null) f.Enabled = !f.Enabled;
                };
            }, "Recoil compensation with ±15% random noise per bullet.");
            b.AddSlider("Compensation", "%", 1, 0, 20, 100, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<NoRecoilNoise>();
                    if (f != null) f.CompensationStrength = (float)s.Slider.Value / 100f;
                };
            }, "How much recoil to compensate.");
            b.AddSlider("Noise Amplitude", "%", 1, 0, 0, 50, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<NoRecoilNoise>();
                    if (f != null) f.NoiseAmplitude = (float)s.Slider.Value / 100f;
                };
            }, "Random deviation added per bullet.");
            b.AddSeparator();
        }

        private void LoadVisibilityLock()
        {
            var b = GetBuilder(VisibilityPanel);
            b.AddTitle("Visibility Lock", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Visibility Lock", VisibilityPanel);
                t.SetMinimizedIcon(_localMinimizeState["Visibility Lock"]);
            });
            b.AddToggle("Contrast-Based Lock", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<VisibilityAimLock>();
                    if (f != null) f.Enabled = !f.Enabled;
                };
            }, "Only aim if target is visually distinct from background.");
            b.AddSlider("Min Contrast", "%", 1, 0, 5, 100, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<VisibilityAimLock>();
                    if (f != null) f.MinContrast = (float)s.Slider.Value;
                };
            }, "Minimum color contrast to allow aiming.");
            b.AddSeparator();
        }

        private void LoadFlickAssist()
        {
            var b = GetBuilder(FlickAssistPanel);
            b.AddTitle("Flick Assist", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Flick Assist", FlickAssistPanel);
                t.SetMinimizedIcon(_localMinimizeState["Flick Assist"]);
            });
            b.AddToggle("Long Flick Assist", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<FlickAssist>();
                    if (f != null) f.Enabled = !f.Enabled;
                };
            }, "Only actives on 60°+ flicks. Adds 15-25% random error.");
            b.AddSlider("Short Flick °", "deg", 1, 0, 5, 60, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<FlickAssist>();
                    if (f != null) f.ShortFlickThreshold = (float)s.Slider.Value;
                };
            }, "Flicks below this angle get zero assist.");
            b.AddSlider("Long Flick °", "deg", 1, 0, 30, 120, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<FlickAssist>();
                    if (f != null) f.LongFlickThreshold = (float)s.Slider.Value;
                };
            }, "Flicks above this angle get full deadzone bypass.");
            b.AddSlider("Error Amount", "%", 1, 0, 0, 50, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<FlickAssist>();
                    if (f != null) f.ErrorRange = (float)s.Slider.Value / 100f;
                };
            }, "Random landing error to appear human.");
            b.AddSlider("Assist Strength", "%", 1, 0, 10, 100, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<FlickAssist>();
                    if (f != null) f.AssistStrength = (float)s.Slider.Value / 100f;
                };
            }, "How much the flick is pulled toward target.");
            b.AddSeparator();
        }

        private void LoadHwidSpoofing()
        {
            var b = GetBuilder(HwidPanel);
            b.AddTitle("HWID Spoofing", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("HWID Spoofing", HwidPanel);
                t.SetMinimizedIcon(_localMinimizeState["HWID Spoofing"]);
            });
            b.AddToggle("Rotate Identity", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<HwidSpoofing>();
                    if (f != null) f.Enabled = !f.Enabled;
                };
            }, "Rotate window class/title every few minutes.");
            b.AddSlider("Rotation Interval", "min", 1, 0, 1, 30, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<HwidSpoofing>();
                    if (f != null) f.RotationIntervalMinutes = (int)s.Slider.Value;
                };
            }, "Minutes between identity rotations.");
            b.AddSeparator();
        }

        private SectionBuilder GetBuilder(StackPanel panel) => new(panel);
    }
}
