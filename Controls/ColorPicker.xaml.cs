using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShadowCheat.Controls
{
    public partial class ColorPicker : Window
    {
        private Color _originalColor;
        private Color _currentColor;
        private bool _isMouseDown;
        private WriteableBitmap? _colorBitmap;

        public event Action<Color>? ColorChanged;

        public ColorPicker(Color initialColor, string title)
        {
            InitializeComponent();
            _originalColor = initialColor;
            _currentColor = initialColor;
            TitleText.Text = title;

            Loaded += (s, e) => CreateColorWheel();
        }

        private void CreateColorWheel()
        {
            int size = 250;
            _colorBitmap = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgra32, null);
            byte[] pixels = new byte[size * size * 4];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double dx = x - size / 2.0;
                    double dy = y - size / 2.0;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= size / 2.0)
                    {
                        double angle = Math.Atan2(dy, dx);
                        double hue = (angle + Math.PI) / (2 * Math.PI) * 360;
                        double saturation = distance / (size / 2.0);
                        var color = HsvToRgb(hue, saturation, 1.0);
                        int offset = (y * size + x) * 4;
                        pixels[offset] = color.B;
                        pixels[offset + 1] = color.G;
                        pixels[offset + 2] = color.R;
                        pixels[offset + 3] = 255;
                    }
                }
            }

            _colorBitmap.WritePixels(new Int32Rect(0, 0, size, size), pixels, size * 4, 0);
            ColorEllipse.Fill = new ImageBrush(_colorBitmap);
            UpdatePreview(_currentColor);
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            UpdateColor(e.GetPosition(ColorCanvas));
            ColorCanvas.CaptureMouse();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown) UpdateColor(e.GetPosition(ColorCanvas));
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
            ColorCanvas.ReleaseMouseCapture();
        }

        private void UpdateColor(Point pos)
        {
            double cx = 125, cy = 125;
            double dx = pos.X - cx, dy = pos.Y - cy;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist > 125) return;

            double angle = Math.Atan2(dy, dx);
            double hue = (angle + Math.PI) / (2 * Math.PI) * 360;
            double saturation = dist / 125.0;
            _currentColor = HsvToRgb(hue, saturation, BrightnessSlider.Value);
            UpdatePreview(_currentColor);
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var (h, s, _) = RgbToHsv(_currentColor);
            _currentColor = HsvToRgb(h, s, e.NewValue);
            UpdatePreview(_currentColor);
        }

        private void HexTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                _currentColor = (Color)ColorConverter.ConvertFromString(HexTextBox.Text);
                UpdatePreview(_currentColor);
            }
            catch { }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ColorChanged?.Invoke(_currentColor);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void UpdatePreview(Color color)
        {
            PreviewEllipse.Fill = new SolidColorBrush(color);
            HexTextBox.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";

            double cx = 125, cy = 125;
            var (h, s, _) = RgbToHsv(color);
            double angle = h * Math.PI / 180;
            double radius = s * 125;
            Canvas.SetLeft(SelectorDot, 125 + radius * Math.Cos(angle) - 8);
            Canvas.SetTop(SelectorDot, 125 + radius * Math.Sin(angle) - 8);
        }

        private static Color HsvToRgb(double hue, double saturation, double value)
        {
            int hi = (int)(hue / 60) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);
            double p = value * (1 - saturation);
            double q = value * (1 - f * saturation);
            double t = value * (1 - (1 - f) * saturation);

            double r, g, b;
            switch (hi)
            {
                case 0: r = value; g = t; b = p; break;
                case 1: r = q; g = value; b = p; break;
                case 2: r = p; g = value; b = t; break;
                case 3: r = p; g = q; b = value; break;
                case 4: r = t; g = p; b = value; break;
                default: r = value; g = p; b = q; break;
            }
            return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        private static (double hue, double saturation, double value) RgbToHsv(Color color)
        {
            double r = color.R / 255.0, g = color.G / 255.0, b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
            double h = 0, s = 0, v = max;
            double d = max - min;
            s = max == 0 ? 0 : d / max;
            if (max != min)
            {
                if (max == r) h = (g - b) / d + (g < b ? 6 : 0);
                else if (max == g) h = (b - r) / d + 2;
                else h = (r - g) / d + 4;
                h /= 6;
            }
            return (h * 360, s, v);
        }
    }
}
