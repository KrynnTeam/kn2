using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShadowCheat.Controls
{
    public partial class AboutPage : UserControl
    {
        private MainWindow? _mainWindow;

        private static readonly (string name, string role, string? avatarUrl)[] CoreTeam =
        {
            ("SHKERS", "Lead Developer", "https://cdn.discordapp.com/avatars/1467424224149901375/5fcd8b870b6f8ac5cf15051a9a1747d3.png?size=128"),
            ("NULL", "UI/UX", "https://cdn.discordapp.com/avatars/1500732517081288725/8d535f159932ab69c9dd488ee1bef864.png?size=128"),
            ("NIX", "Performance", "https://media.discordapp.net/attachments/1461960080454062091/1518065650579275856/e445ebbaa8a34a6b0ef17be015e71649.png?ex=6a38904f&is=6a373ecf&hm=6944de42862d7afd3de5e263270334402dbbf83fae426d0e8242bf5d6a4aa14e&=&format=webp&quality=lossless"),
        };

        public AboutPage() { InitializeComponent(); }

        public void Initialize(MainWindow mainWindow)
        {
            if (_mainWindow != null) return;
            _mainWindow = mainWindow;

            LoadCoreTeam();
            _ = LoadSystemSpecsAsync();
        }

        private void LoadCoreTeam()
        {
            CoreTeamPanel.Children.Clear();
            foreach (var (name, role, avatarUrl) in CoreTeam)
            {
                var card = new Border
                {
                    Width = 110,
                    Margin = new Thickness(6, 0, 6, 0),
                    Background = Brushes.Transparent,
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(8)
                };

                var innerStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

                var avatarBorder = new Border
                {
                    Width = 48,
                    Height = 48,
                    CornerRadius = new CornerRadius(24),
                    Background = new SolidColorBrush(Color.FromRgb(0x72, 0x2E, 0xD1)),
                    Margin = new Thickness(0, 0, 0, 8)
                };
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
                        avatarBorder.Child = CreateInitialText(name);
                    }
                }
                else
                {
                    avatarBorder.Child = CreateInitialText(name);
                }

                innerStack.Children.Add(avatarBorder);
                innerStack.Children.Add(new TextBlock
                {
                    Text = name,
                    FontSize = 12,
                    Foreground = Brushes.White,
                    FontFamily = (FontFamily)FindResource("AtkinsonHyperlegible"),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                innerStack.Children.Add(new TextBlock
                {
                    Text = role,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromArgb(0x70, 0xFF, 0xFF, 0xFF)),
                    FontFamily = (FontFamily)FindResource("AtkinsonHyperlegible"),
                    HorizontalAlignment = HorizontalAlignment.Center
                });

                card.Child = innerStack;
                CoreTeamPanel.Children.Add(card);
            }
        }

        private static TextBlock CreateInitialText(string name)
        {
            return new TextBlock
            {
                Text = name[0].ToString().ToUpper(),
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private async System.Threading.Tasks.Task LoadSystemSpecsAsync()
        {
            try
            {
                var specs = await System.Threading.Tasks.Task.Run(() =>
                {
                    string cpu = "", gpu = "", ram = "";
                    using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                        foreach (var obj in searcher.Get()) cpu = obj["Name"]?.ToString() ?? "Unknown";
                    using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                        foreach (var obj in searcher.Get()) { gpu = obj["Name"]?.ToString() ?? "Unknown"; break; }
                    using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                        foreach (var obj in searcher.Get())
                        {
                            long bytes = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                            ram = $"{bytes / (1024 * 1024 * 1024.0):F1} GB";
                            break;
                        }
                    return $"CPU: {cpu}\nGPU: {gpu}\nRAM: {ram}";
                });
                AboutSpecs.Text = specs;
            }
            catch
            {
                AboutSpecs.Text = "System specs unavailable";
            }
        }

        private void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(new ProcessStartInfo { FileName = "https://github.com", UseShellExecute = true }); }
            catch { }
        }

        private void DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(new ProcessStartInfo { FileName = "https://discord.gg", UseShellExecute = true }); }
            catch { }
        }
    }
}
