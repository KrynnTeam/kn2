using System.Diagnostics;
using System.Management;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShadowCheat.Controls
{
    public partial class AboutMenuControl : UserControl
    {
        private MainWindow? _mainWindow;
        private bool _isInitialized;
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };

        private static readonly (string name, string role, string? github, string? avatarUrl)[] CoreTeam =
        {
            ("SHKERS", "Lead Developer", null, "https://cdn.discordapp.com/avatars/1467424224149901375/5fcd8b870b6f8ac5cf15051a9a1747d3.png?size=128"),
            ("NULL", "UI/UX", null, "https://cdn.discordapp.com/avatars/1500732517081288725/8d535f159932ab69c9dd488ee1bef864.png?size=128"),
            ("NIX", "Performance", null, "https://media.discordapp.net/attachments/1461960080454062091/1518065650579275856/e445ebbaa8a34a6b0ef17be015e71649.png?ex=6a38904f&is=6a373ecf&hm=6944de42862d7afd3de5e263270334402dbbf83fae426d0e8242bf5d6a4aa14e&=&format=webp&quality=lossless"),
        };

        public Label AboutSpecsControl => AboutSpecs;
        public ScrollViewer AboutMenuScrollViewer => AboutMenu;

        public AboutMenuControl() { InitializeComponent(); }

        public void Initialize(MainWindow mainWindow)
        {
            if (_isInitialized) return;
            _mainWindow = mainWindow;
            _isInitialized = true;

            LoadCoreTeam();
            LoadSystemSpecs();
        }

        private void LoadCoreTeam()
        {
            CoreTeamPanel.Children.Clear();
            foreach (var (name, role, github, avatarUrl) in CoreTeam)
            {
                var panel = new StackPanel { Margin = new Thickness(8, 0, 8, 0), HorizontalAlignment = HorizontalAlignment.Center };
                var avatarBorder = new Border { Width = 48, Height = 48, CornerRadius = new CornerRadius(24),
                    Background = new SolidColorBrush(Color.FromRgb(0x8B, 0x00, 0x00)), Margin = new Thickness(0, 0, 0, 8) };
                avatarBorder.Clip = new EllipseGeometry { Center = new Point(24, 24), RadiusX = 24, RadiusY = 24 };

                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(avatarUrl);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        avatarBorder.Child = new Image { Source = bitmap, Stretch = Stretch.UniformToFill };
                    }
                    catch
                    {
                        avatarBorder.Child = new TextBlock { Text = name[0].ToString().ToUpper(), FontSize = 20,
                            FontWeight = FontWeights.Bold, Foreground = Brushes.White,
                            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                    }
                }
                else
                {
                    avatarBorder.Child = new TextBlock { Text = name[0].ToString().ToUpper(), FontSize = 20,
                        FontWeight = FontWeights.Bold, Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                }

                panel.Children.Add(avatarBorder);
                panel.Children.Add(new TextBlock { Text = name, FontSize = 12, Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center });
                panel.Children.Add(new TextBlock { Text = role, FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromArgb(0x70, 0xFF, 0xFF, 0xFF)),
                    HorizontalAlignment = HorizontalAlignment.Center });
                CoreTeamPanel.Children.Add(panel);
            }
        }

        private void LoadSystemSpecs()
        {
            try
            {
                string cpu = "Unknown";
                string gpu = "Unknown";
                string ram = "Unknown";

                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                    foreach (var obj in searcher.Get())
                        cpu = obj["Name"]?.ToString() ?? "Unknown";

                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                    foreach (var obj in searcher.Get())
                        { gpu = obj["Name"]?.ToString() ?? "Unknown"; break; }

                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                    foreach (var obj in searcher.Get())
                    {
                        var totalRAM = Convert.ToDouble(obj["TotalPhysicalMemory"]) / (1024 * 1024 * 1024);
                        ram = $"{totalRAM:F1} GB";
                    }

                AboutSpecs.Content = $"{cpu} | {gpu} | {ram}";
            }
            catch { AboutSpecs.Content = "System specs unavailable"; }
        }

        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e) { }
        private void GitHubButton_Click(object sender, RoutedEventArgs e) { try { Process.Start(new ProcessStartInfo { FileName = "https://github.com", UseShellExecute = true }); } catch { } }
        private void DiscordButton_Click(object sender, RoutedEventArgs e) { try { Process.Start(new ProcessStartInfo { FileName = "https://discord.gg", UseShellExecute = true }); } catch { } }
    }
}
