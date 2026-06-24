using ShadowCheat.Class;
using ShadowCheat.UILibrary;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace ShadowCheat.Controls
{
    public partial class VisualMenuControl : UserControl
    {
        private MainWindow? _mainWindow;
        private bool _isInitialized;
        private readonly Dictionary<string, bool> _localMinimizeState = new()
        {
            { "Overlay Display", false }, { "Scan Radius", false }, { "Overlay Style", false }
        };

        public StackPanel OverlayPanel_ => OverlayPanel;
        public StackPanel ScanRadiusPanel_ => ScanRadiusPanel;
        public StackPanel OverlayStylePanel_ => OverlayStylePanel;

        public VisualMenuControl() { InitializeComponent(); }

        public void Initialize(MainWindow mainWindow)
        {
            if (_isInitialized) return;
            _mainWindow = mainWindow;
            _isInitialized = true;

            LoadOverlay();
            LoadScanRadius();
            LoadOverlayStyle();
            ApplyMinimizeStates();
        }

        private void ApplyMinimizeStates()
        {
            ApplyPanelState("Overlay Display", OverlayPanel);
            ApplyPanelState("Scan Radius", ScanRadiusPanel);
            ApplyPanelState("Overlay Style", OverlayStylePanel);
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

        private void LoadOverlay()
        {
            var b = new SectionBuilder(OverlayPanel);
            b.AddTitle("Overlay Display", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Overlay Display", OverlayPanel);
                t.SetMinimizedIcon(_localMinimizeState["Overlay Display"]);
            });
            b.AddToggle("Show Overlay", t =>
            {
                t.EnableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    _mainWindow?.ToggleOverlayVisibility();
                };
            }, "Show or hide the overlay window.");
            b.AddToggle("Target Box", t =>
            {
                t.EnableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    var overlay = _mainWindow?.GetOverlay();
                    if (overlay != null)
                    {
                        overlay.ShowTargetBox = !overlay.ShowTargetBox;
                        if (overlay.ShowTargetBox) t.EnableSwitch(); else t.DisableSwitch();
                    }
                };
            }, "Draw a box around the detected target.");
            b.AddToggle("FOV Circle", t =>
            {
                t.EnableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    var overlay = _mainWindow?.GetOverlay();
                    if (overlay != null)
                    {
                        overlay.ShowFov = !overlay.ShowFov;
                        if (overlay.ShowFov) t.EnableSwitch(); else t.DisableSwitch();
                    }
                };
            }, "Show the scan radius circle.");
            b.AddToggle("Crosshair Lines", t =>
            {
                t.EnableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    var overlay = _mainWindow?.GetOverlay();
                    if (overlay != null)
                    {
                        overlay.ShowCrosshairLines = !overlay.ShowCrosshairLines;
                        if (overlay.ShowCrosshairLines) t.EnableSwitch(); else t.DisableSwitch();
                    }
                };
            }, "Show crosshair lines on the overlay.");
            b.AddSeparator();
        }

        private void LoadScanRadius()
        {
            var b = new SectionBuilder(ScanRadiusPanel);
            b.AddTitle("Scan Radius", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Scan Radius", ScanRadiusPanel);
                t.SetMinimizedIcon(_localMinimizeState["Scan Radius"]);
            });
            b.AddSlider("Radius", "px", 1, 1, 50, 400, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var detector = _mainWindow?.FeatureManager.Detector;
                    if (detector != null) detector.Profile.ScanRadius = (int)s.Slider.Value;
                };
            }, "Radius of the scan area around crosshair.");
            b.AddSlider("Minimum Confidence", "%", 1, 1, 10, 95, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var detector = _mainWindow?.FeatureManager.Detector;
                    if (detector != null) detector.Profile.MinConfidence = (float)(s.Slider.Value / 100.0);
                };
            }, "Minimum color match confidence (0-100).");
            b.AddSlider("Minimum Contrast", "%", 1, 1, 10, 95, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var detector = _mainWindow?.FeatureManager.Detector;
                    if (detector != null) detector.Profile.MinContrast = (float)(s.Slider.Value / 100.0);
                };
            }, "Minimum contrast between target and background.");
            b.AddSeparator();
        }

        private void LoadOverlayStyle()
        {
            var b = new SectionBuilder(OverlayStylePanel);
            b.AddTitle("Overlay Style", true, t => t.Minimize.Click += (_, _) =>
            {
                TogglePanel("Overlay Style", OverlayStylePanel);
                t.SetMinimizedIcon(_localMinimizeState["Overlay Style"]);
            });
            b.AddToggle("Topmost", t =>
            {
                t.EnableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    var overlay = _mainWindow?.GetOverlay();
                    if (overlay != null)
                    {
                        overlay.Topmost = !overlay.Topmost;
                        if (overlay.Topmost) t.EnableSwitch(); else t.DisableSwitch();
                    }
                };
            }, "Keep overlay on top of all windows.");
            b.AddToggle("Click-Through", t =>
            {
                t.Reader.Click += (_, _) =>
                {
                    var overlay = _mainWindow?.GetOverlay();
                    if (overlay != null)
                    {
                        overlay.IsHitTestVisible = !overlay.IsHitTestVisible;
                        if (!overlay.IsHitTestVisible) t.EnableSwitch(); else t.DisableSwitch();
                    }
                };
            }, "Pass clicks through the overlay.");
            b.AddSeparator();
        }
    }
}
