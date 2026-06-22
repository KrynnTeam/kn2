using ShadowCheat.Class;
using ShadowCheat.Class.Features;
using ShadowCheat.UILibrary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShadowCheat.Controls
{
    public partial class SettingsMenuControl : UserControl
    {
        private MainWindow? _mainWindow;
        private bool _isInitialized;
        private bool _autoStart, _minimizeToTray, _darkMode = true;

        public StackPanel ModelSettingsPanel => ModelSettings;
        public StackPanel SettingsConfigPanel => SettingsConfig;
        public StackPanel ThemeMenuPanel => ThemeMenu;
        public StackPanel DisplaySelectMenuPanel => DisplaySelectMenu;

        public SettingsMenuControl() { InitializeComponent(); }

        public void Initialize(MainWindow mainWindow)
        {
            if (_isInitialized) return;
            _mainWindow = mainWindow;
            _isInitialized = true;

            LoadGeneralSettings();
            LoadDisplayConfig();
            LoadModelConfig();
            LoadThemeConfig();
        }

        private void LoadGeneralSettings()
        {
            var b = new SectionBuilder(SettingsConfig);
            b.AddTitle("General Settings", true, t => { });

            b.AddToggle("Auto-Start", t =>
            {
                t.Reader.Click += (_, _) => _autoStart = !_autoStart;
            }, "Automatically start modules on launch.");

            b.AddToggle("Minimize to Tray", t =>
            {
                t.Reader.Click += (_, _) => _minimizeToTray = !_minimizeToTray;
            }, "Minimize to system tray instead of taskbar.");

            b.AddToggle("Show Overlay", t =>
            {
                t.EnableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    var manager = _mainWindow?.FeatureManager;
                    if (manager != null)
                    {
                        if (manager.IsRunning)
                            manager.Stop();
                        else
                            manager.Start();
                    }
                };
            }, "Display HUD overlay in-game.");

            b.AddSeparator();
            b.AddTitle("Performance", true, t => { });

            b.AddSlider("Update Rate", "ms", 1, 1, 10, 100, s =>
            {
                s.Slider.ValueChanged += (_, _) =>
                {
                    var detector = _mainWindow?.FeatureManager.Detector;
                    if (detector != null)
                        detector.Profile.ScanRadius = (int)s.Slider.Value;
                };
            }, "Detection loop interval.");

            b.AddSlider("Priority Level", "", 1, 1, 1, 5, s => { }, "Process priority (1=Low, 5=High).");
            b.AddSeparator();
        }

        private void LoadDisplayConfig()
        {
            var b = new SectionBuilder(DisplaySelectMenu);
            b.AddTitle("Display Configuration", false);
            b.AddButton("Select Display", btn =>
            {
                btn.Reader.Click += (_, _) =>
                {
                    var selector = new ADisplaySelector();
                    var win = new Window
                    {
                        Title = "Display Selection",
                        Content = selector,
                        Width = 320, Height = 260,
                        WindowStyle = WindowStyle.None,
                        AllowsTransparency = true,
                        Background = Brushes.Transparent,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        ResizeMode = ResizeMode.NoResize
                    };
                    selector.DisplayGrid.MouseLeftButtonUp += (_, _) =>
                    {
                        var info = selector.GetSelectedDisplayIndex();
                        btn.ButtonTitle.Content = $"Display {info + 1}";
                        win.Close();
                    };
                    win.ShowDialog();
                };
            });
            b.AddSeparator();
        }

        private void LoadModelConfig()
        {
            var b = new SectionBuilder(ModelSettings);
            b.AddTitle("AI Model Settings", true, t => { });

            b.AddFileLocator("Model Path", loc =>
            {
                loc.OpenFileB.Click += (_, _) =>
                {
                    var dialog = new Microsoft.Win32.OpenFileDialog
                    {
                        Filter = "Model files (*.onnx;*.pt)|*.onnx;*.pt|All files (*.*)|*.*"
                    };
                    if (dialog.ShowDialog() == true)
                        loc.FileLocationTextbox.Text = dialog.FileName;
                };
            });

            b.AddDropdown("Model Type", d =>
            {
                d.DropdownBox.Items.Add("YOLOv8");
                d.DropdownBox.Items.Add("YOLOv5");
                d.DropdownBox.Items.Add("Custom ONNX");
                d.DropdownBox.SelectedIndex = 0;
            });

            b.AddSlider("Confidence", "%", 0.05, 0.05, 0.0, 1.0, s =>
            {
                s.Slider.Value = 0.5;
                s.Slider.ValueChanged += (_, _) =>
                {
                    var detector = _mainWindow?.FeatureManager.Detector;
                    if (detector != null)
                        detector.Profile.MinConfidence = (float)s.Slider.Value;
                };
            }, "Minimum confidence threshold.");

            b.AddToggle("Auto-Download Models", t => { }, "Fetch models from repository.");
            b.AddSeparator();
        }

        private void LoadThemeConfig()
        {
            var b = new SectionBuilder(ThemeMenu);
            b.AddTitle("Appearance", false);

            b.AddColorChanger("Theme Color", c =>
            {
                c.Reader.Click += (_, _) =>
                {
                    var picker = new ColorPicker(Color.FromRgb(0xCC, 0x00, 0x00), "Theme Color");
                    picker.ColorChanged += (color) =>
                    {
                        _mainWindow?.ApplyThemeColor(color);
                    };
                    picker.ShowDialog();
                };
            });

            b.AddToggle("Dark Mode", t =>
            {
                t.EnableSwitch();
                t.Reader.Click += (_, _) =>
                {
                    _darkMode = !_darkMode;
                };
            }, "Dark interface theme.");

            b.AddSeparator();
        }
    }
}
