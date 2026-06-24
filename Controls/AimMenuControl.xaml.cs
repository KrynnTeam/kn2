using ShadowCheat.Class;
using ShadowCheat.Class.Features;
using ShadowCheat.UILibrary;
using System;
using System.Drawing;
using System.Threading.Tasks;
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

        private void WireToggle<T>(AToggle t, string displayName) where T : FeatureBase, new()
        {
            t.Reader.Click += (_, _) =>
            {
                var f = _mainWindow!.FeatureManager.GetFeature<T>();
                if (f == null) return;
                bool newState = !f.Enabled;
                f.Enabled = newState;
                if (newState) t.EnableSwitch(); else t.DisableSwitch();
                _mainWindow.ShowNotification(displayName, newState);
            };
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
                    var profile = fm.Detector.Profile;
                    bool isEnabled = profile.MinConfidence < 1.0f;
                    if (isEnabled)
                    {
                        profile.MinConfidence = 1.01f;
                        t.DisableSwitch();
                        _mainWindow.ShowNotification("Target Detection", false);
                    }
                    else
                    {
                        profile.MinConfidence = 0.3f;
                        t.EnableSwitch();
                        _mainWindow.ShowNotification("Target Detection", true);
                    }
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
                    try
                    {
                        var picker = new ColorPicker(
                            ((System.Windows.Media.SolidColorBrush)c.ColorChangingBorder.Background).Color, "Enemy Color");
                        picker.ColorChanged += color =>
                        {
                            c.ColorChangingBorder.Background = new System.Windows.Media.SolidColorBrush(color);
                            _mainWindow!.FeatureManager.Detector.SetColor(color.R, color.G, color.B);
                        };
                        picker.ShowDialog();
                    }
                    catch
                    {
                        using var cd = new System.Windows.Forms.ColorDialog
                        {
                            Color = System.Drawing.Color.FromArgb(profile.TargetR, profile.TargetG, profile.TargetB),
                            FullOpen = true
                        };
                        if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            var wpfColor = System.Windows.Media.Color.FromRgb(cd.Color.R, cd.Color.G, cd.Color.B);
                            c.ColorChangingBorder.Background = new System.Windows.Media.SolidColorBrush(wpfColor);
                            _mainWindow!.FeatureManager.Detector.SetColor(cd.Color.R, cd.Color.G, cd.Color.B);
                        }
                    }
                };
            });
            b.AddButton("Pick from Screen", btn =>
            {
                btn.Reader.Click += async (_, _) =>
                {
                    btn.ButtonTitle.Content = "3...";
                    await Task.Delay(1000);
                    btn.ButtonTitle.Content = "2...";
                    await Task.Delay(1000);
                    btn.ButtonTitle.Content = "1...";
                    await Task.Delay(1000);

                    var cursorPos = System.Windows.Forms.Cursor.Position;
                    using var bmp = new System.Drawing.Bitmap(1, 1);
                    using var g = System.Drawing.Graphics.FromImage(bmp);
                    g.CopyFromScreen(cursorPos.X, cursorPos.Y, 0, 0, new System.Drawing.Size(1, 1));
                    var pixel = bmp.GetPixel(0, 0);

                    var profile = _mainWindow!.FeatureManager.Detector.Profile;
                    profile.TargetR = pixel.R;
                    profile.TargetG = pixel.G;
                    profile.TargetB = pixel.B;

                    foreach (var child in DetectionPanel.Children)
                    {
                        if (child is AColorChanger acc)
                        {
                            acc.ColorChangingBorder.Background =
                                new System.Windows.Media.SolidColorBrush(
                                    System.Windows.Media.Color.FromRgb(pixel.R, pixel.G, pixel.B));
                            break;
                        }
                    }

                    btn.ButtonTitle.Content = $"Sampled RGB({pixel.R},{pixel.G},{pixel.B})";
                    _ = Task.Delay(2000).ContinueWith(_ =>
                        btn.Dispatcher.Invoke(() => btn.ButtonTitle.Content = "Pick from Screen"));
                };
            });
            b.AddSeparator();
        }

        private void LoadCrosshairPlacement()
        {
            var b = GetBuilder(CrosshairAssistPanel);
            b.AddTitle("Aim Assist", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Crosshair Placement", CrosshairAssistPanel);
                t.SetMinimizedIcon(_localMinimizeState["Crosshair Placement"]);
            });
            b.AddToggle("Aim Assist", t =>
            {
                WireToggle<CrosshairPlacementAssist>(t, "Aim Assist");
            }, "Gently pull crosshair toward target. No snap.");
            var isCross = _mainWindow!.FeatureManager.Detector.Profile.Mode == DetectionMode.Crosshair;
            b.AddButton(isCross ? "Mode: Crosshair" : "Mode: Color", btn =>
            {
                btn.ButtonTitle.Content = isCross ? "Mode: Crosshair" : "Mode: Color";
                btn.Reader.Click += (_, _) =>
                {
                    var profile = _mainWindow!.FeatureManager.Detector.Profile;
                    profile.Mode = profile.Mode == DetectionMode.Color ? DetectionMode.Crosshair : DetectionMode.Color;
                    profile.ResetBaseline();
                    btn.ButtonTitle.Content = profile.Mode == DetectionMode.Crosshair ? "Mode: Crosshair" : "Mode: Color";
                };
            });
            b.AddButton("Aim Key: Right Click", btn =>
            {
                var aimFeature2 = _mainWindow!.FeatureManager.GetFeature<CrosshairPlacementAssist>();
                if (aimFeature2 != null)
                    btn.ButtonTitle.Content = $"Aim Key: {KeyName(aimFeature2.AimKey)}";
                btn.Reader.Click += (_, _) =>
                {
                    btn.ButtonTitle.Content = "Press a key...";
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(80)
                    };
                    timer.Tick += (_, _) =>
                    {
                        for (int vk = 1; vk <= 254; vk++)
                        {
                            if (!InputSimulator.IsKeyDown(vk)) continue;
                            timer.Stop();
                            var f = _mainWindow!.FeatureManager.GetFeature<CrosshairPlacementAssist>();
                            if (f != null) f.AimKey = vk;
                            btn.ButtonTitle.Content = $"Aim Key: {KeyName(vk)}";
                            return;
                        }
                    };
                    timer.Start();
                };
            });
            b.AddToggle("Automatic (Experimental)", t =>
            {
                var aimFeature3 = _mainWindow!.FeatureManager.GetFeature<CrosshairPlacementAssist>();
                if (aimFeature3 != null && !aimFeature3.RequireAimKey)
                    t.EnableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<CrosshairPlacementAssist>();
                    if (f == null) return;
                    f.RequireAimKey = !f.RequireAimKey;
                    if (!f.RequireAimKey) t.EnableSwitch(); else t.DisableSwitch();
                    _mainWindow.ShowNotification("Automatic Aim", !f.RequireAimKey);
                };
            }, "No key required. Experimental — may cause unintended movement.");
            b.AddSlider("Crosshair Tolerance", "%", 1, 1, 10, 100, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    _mainWindow!.FeatureManager.Detector.Profile.CrosshairTolerance = (byte)s.Slider.Value;
                };
            }, "Higher = more sensitive to center pixel changes.");
            b.AddSlider("Aim FOV", "px", 10, 0, 30, 400, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<CrosshairPlacementAssist>();
                    if (f != null) f.AimFov = (float)s.Slider.Value;
                };
            }, "Radius around crosshair to assist aiming.");
            b.AddSlider("Smoothing", "%", 1, 0, 15, 100, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<CrosshairPlacementAssist>();
                    if (f != null) f.Smoothing = (float)s.Slider.Value / 100f;
                };
            }, "Higher = faster aim, Lower = smoother human-like.");
            b.AddSlider("Drag Strength", "x", 0.01, 0, 0.01, 0.60, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<CrosshairPlacementAssist>();
                    if (f != null) f.DragStrength = (float)s.Slider.Value;
                };
            }, "How strong the pull feels per frame.");
            b.AddSlider("Deadzone", "px", 1, 0, 5, 30, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var f = _mainWindow!.FeatureManager.GetFeature<CrosshairPlacementAssist>();
                    if (f != null) f.Deadzone = (float)s.Slider.Value;
                };
            }, "Deadzone from center — no assist if target is this close.");
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
                WireToggle<StandstillAccuracy>(t, "Standstill Accuracy");
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
                WireToggle<ShotOverride>(t, "Shot Override");
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
                WireToggle<NoRecoilNoise>(t, "No-Recoil Noise");
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
                WireToggle<VisibilityAimLock>(t, "Visibility Lock");
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
                WireToggle<FlickAssist>(t, "Flick Assist");
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
                WireToggle<HwidSpoofing>(t, "HWID Spoofing");
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

        private static string KeyName(int vk) => vk switch
        {
            0x01 => "Left Click",
            0x02 => "Right Click",
            0x04 => "Middle Click",
            0x05 => "X1 Click",
            0x06 => "X2 Click",
            >= 0x07 and <= 0xA5 => ((System.Windows.Input.Key)vk).ToString(),
            _ => $"VK 0x{vk:X}"
        };
    }
}
