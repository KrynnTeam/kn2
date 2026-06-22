using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShadowCheat.UILibrary
{
    public class DisplayInfo
    {
        public int Index { get; set; }
        public bool IsPrimary { get; set; }
        public System.Windows.Rect Bounds { get; set; }
    }

    public partial class ADisplaySelector : UserControl
    {
        private List<DisplayInfo> _displays = new();
        private int _selectedDisplayIndex = 0;
        private static readonly Color ThemeColor = Color.FromRgb(0x00, 0xA8, 0xFF);

        public ADisplaySelector()
        {
            InitializeComponent();
            Loaded += (s, e) => RefreshDisplays();
        }

        public void RefreshDisplays()
        {
            _displays.Clear();
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                var bounds = screen.Bounds;
                _displays.Add(new DisplayInfo
                {
                    Index = _displays.Count,
                    IsPrimary = screen.Primary,
                    Bounds = new System.Windows.Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height)
                });
            }

            DisplayGrid.Children.Clear();
            foreach (var display in _displays)
            {
                var container = new Border
                {
                    Margin = new Thickness(5),
                    Background = new SolidColorBrush(Color.FromArgb(51, 60, 60, 60)),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(63, 255, 255, 255)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Cursor = Cursors.Hand,
                    Tag = display.Index
                };

                var grid = new Grid();

                var monitorBorder = new Border
                {
                    Width = 50, Height = 35,
                    Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(3),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 25)
                };

                var stand = new Rectangle
                {
                    Width = 20, Height = 8,
                    Fill = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 35, 0, 0)
                };

                var displayNumber = new TextBlock
                {
                    Text = (display.Index + 1).ToString(),
                    FontSize = 16, FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromArgb(221, 255, 255, 255)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 15)
                };

                grid.Children.Add(monitorBorder);
                grid.Children.Add(stand);
                grid.Children.Add(displayNumber);

                if (display.IsPrimary)
                {
                    var primaryBadge = new Border
                    {
                        Background = new SolidColorBrush(ThemeColor),
                        CornerRadius = new CornerRadius(3),
                        Padding = new Thickness(4, 2, 4, 2),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    primaryBadge.Child = new TextBlock
                    {
                        Text = "Primary", FontSize = 9, Foreground = Brushes.White
                    };
                    grid.Children.Add(primaryBadge);
                }

                container.Child = grid;
                container.MouseLeftButtonDown += (s, e) =>
                {
                    if (s is Border b && b.Tag is int idx)
                    {
                        _selectedDisplayIndex = idx;
                        UpdateUI();
                    }
                };
                DisplayGrid.Children.Add(container);
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            for (int i = 0; i < DisplayGrid.Children.Count; i++)
            {
                if (DisplayGrid.Children[i] is Border border && border.Tag is int idx)
                {
                    bool isSelected = idx == _selectedDisplayIndex;
                    border.Background = isSelected
                        ? new SolidColorBrush(ThemeColor)
                        : new SolidColorBrush(Color.FromArgb(51, 60, 60, 60));
                    border.BorderBrush = isSelected
                        ? new SolidColorBrush(Colors.White)
                        : new SolidColorBrush(Color.FromArgb(63, 255, 255, 255));
                    border.BorderThickness = new Thickness(isSelected ? 2 : 1);
                }
            }

            if (_selectedDisplayIndex < _displays.Count)
            {
                var d = _displays[_selectedDisplayIndex];
                CurrentDisplayInfo.Content = $"Display {d.Index + 1} Selected{(d.IsPrimary ? " (Primary)" : "")} - {d.Bounds.Width}x{d.Bounds.Height}";
            }
        }

        public int GetSelectedDisplayIndex() => _selectedDisplayIndex;
    }
}
