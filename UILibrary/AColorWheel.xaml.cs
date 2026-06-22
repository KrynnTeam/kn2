using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShadowCheat.UILibrary
{
    public partial class AColorWheel : UserControl
    {
        private bool _isMouseDown;
        private WriteableBitmap? _colorWheelBitmap;
        private Color _selectedColor = Color.FromRgb(114, 46, 209);
        private double _brightness = 1.0;
        private bool _isUpdatingFromCode;
        private bool _isShowingDragDrop;

        public event Action<Color>? ColorChanged;

        public AColorWheel()
        {
            InitializeComponent();
            Loaded += (s, e) => CreateColorWheel();
        }

        private void CreateColorWheel()
        {
            int size = 200;
            _colorWheelBitmap = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgra32, null);
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
                        Color color = HsvToRgb(hue, saturation, 1.0);
                        int offset = (y * size + x) * 4;
                        pixels[offset] = color.B;
                        pixels[offset + 1] = color.G;
                        pixels[offset + 2] = color.R;
                        pixels[offset + 3] = 255;
                    }
                }
            }

            _colorWheelBitmap.WritePixels(new Int32Rect(0, 0, size, size), pixels, size * 4, 0);
            ColorWheelEllipse.Fill = new ImageBrush(_colorWheelBitmap);
            UpdateColorPreview(_selectedColor);
            UpdateBrightnessGradient();
        }

        private void ColorWheel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            UpdateColorFromPosition(e.GetPosition(ColorWheelCanvas));
            ColorWheelCanvas.CaptureMouse();
        }

        private void ColorWheel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown) UpdateColorFromPosition(e.GetPosition(ColorWheelCanvas));
        }

        private void ColorWheel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
            ColorWheelCanvas.ReleaseMouseCapture();
        }

        private void UpdateColorFromPosition(Point position)
        {
            double cx = 100, cy = 100;
            double dx = position.X - cx;
            double dy = position.Y - cy;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance > 100) return;

            double angle = Math.Atan2(dy, dx);
            double hue = (angle + Math.PI) / (2 * Math.PI) * 360;
            double saturation = distance / 100.0;

            _selectedColor = HsvToRgb(hue, saturation, _brightness);
            UpdateUI();
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (BrightnessValue == null) return;
            _brightness = e.NewValue;
            BrightnessValue.Text = ((int)(_brightness * 100)).ToString();
            var (h, s, _) = RgbToHsv(_selectedColor);
            _selectedColor = HsvToRgb(h, s, _brightness);
            UpdateUI();
        }

        private void BrightnessValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingFromCode) return;
            if (int.TryParse(BrightnessValue.Text, out int val))
            {
                _brightness = Math.Clamp(val / 100.0, 0, 1);
                _isUpdatingFromCode = true;
                BrightnessSlider.Value = _brightness;
                _isUpdatingFromCode = false;
            }
        }

        private void HexValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingFromCode) return;
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(HexValue.Text);
                _selectedColor = color;
                var (h, s, _) = RgbToHsv(color);
                _brightness = 1.0;
                _isUpdatingFromCode = true;
                BrightnessSlider.Value = 1.0;
                BrightnessValue.Text = "100";
                _isUpdatingFromCode = false;
                UpdateUI();
            }
            catch { }
        }

        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            _isShowingDragDrop = !_isShowingDragDrop;
            ColorWheelView.Visibility = _isShowingDragDrop ? Visibility.Collapsed : Visibility.Visible;
            DragDropView.Visibility = _isShowingDragDrop ? Visibility.Visible : Visibility.Collapsed;
            ArrowButton.Content = _isShowingDragDrop ? "<" : ">";
        }

        private void UpdateUI()
        {
            UpdateColorPreview(_selectedColor);
            PositionSelectorForColor(_selectedColor);
            UpdateBrightnessGradient();
            _isUpdatingFromCode = true;
            HexValue.Text = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}";
            _isUpdatingFromCode = false;
            ColorChanged?.Invoke(_selectedColor);
        }

        private void UpdateColorPreview(Color color)
        {
            ColorPreview.Fill = new SolidColorBrush(color);
            ColorDot.Fill = new SolidColorBrush(color);
        }

        private void UpdateBrightnessGradient()
        {
            BrightnessGradientEnd.Color = _selectedColor;
        }

        private void PositionSelectorForColor(Color color)
        {
            var (hue, saturation, _) = RgbToHsv(color);
            double angle = hue * Math.PI / 180;
            double radius = saturation * 100;

            double x = 100 + radius * Math.Cos(angle) - 10;
            double y = 100 + radius * Math.Sin(angle) - 10;

            Canvas.SetLeft(ColorSelector, Math.Clamp(x, -5, 185));
            Canvas.SetTop(ColorSelector, Math.Clamp(y, -5, 185));
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
