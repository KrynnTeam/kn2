using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShadowCheat.Class
{
    public partial class OverlayWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_TOOLWINDOW = 0x80;
        private const uint LWA_ALPHA = 0x2;

        private readonly System.Windows.Controls.Canvas _canvas;
        private Ellipse? _fovCircle;
        private System.Windows.Shapes.Rectangle? _targetBox;
        private System.Windows.Shapes.Rectangle? _targetBoxOutline;

        public bool ShowFov { get; set; } = true;
        public float FovRadius { get; set; } = 150f;
        public Color FovColor { get; set; } = Color.FromArgb(0x44, 0x72, 0x2E, 0xD1);
        public bool ShowTargetBox { get; set; } = true;
        public float TargetX { get; set; }
        public float TargetY { get; set; }
        public float TargetW { get; set; }
        public float TargetH { get; set; }

        public OverlayWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            Topmost = true;
            ShowInTaskbar = false;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
            Left = 0;
            Top = 0;
            WindowState = WindowState.Maximized;
            Cursor = Cursors.None;
            IsHitTestVisible = false;

            _canvas = new System.Windows.Controls.Canvas();
            Content = _canvas;

            CreateFovCircle();
            CreateTargetBox();

            Loaded += OnLoaded;
        }

        private void CreateFovCircle()
        {
            _fovCircle = new Ellipse
            {
                Width = FovRadius * 2,
                Height = FovRadius * 2,
                Stroke = new SolidColorBrush(FovColor),
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(0x11, 0x72, 0x2E, 0xD1)),
                IsHitTestVisible = false
            };
            _canvas.Children.Add(_fovCircle);
            System.Windows.Controls.Canvas.SetLeft(_fovCircle, (Width / 2) - FovRadius);
            System.Windows.Controls.Canvas.SetTop(_fovCircle, (Height / 2) - FovRadius);
        }

        private void CreateTargetBox()
        {
            _targetBoxOutline = new System.Windows.Shapes.Rectangle
            {
                Stroke = new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0x00, 0x00)),
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(0x22, 0xFF, 0x00, 0x00)),
                IsHitTestVisible = false,
                Visibility = Visibility.Collapsed,
                RadiusX = 2,
                RadiusY = 2
            };
            _canvas.Children.Add(_targetBoxOutline);
        }

        public void UpdateOverlay()
        {
            if (!IsVisible) return;

            if (ShowFov && _fovCircle != null)
            {
                _fovCircle.Visibility = Visibility.Visible;
                _fovCircle.Width = FovRadius * 2;
                _fovCircle.Height = FovRadius * 2;
                System.Windows.Controls.Canvas.SetLeft(_fovCircle, (Width / 2) - FovRadius);
                System.Windows.Controls.Canvas.SetTop(_fovCircle, (Height / 2) - FovRadius);
            }
            else if (_fovCircle != null)
            {
                _fovCircle.Visibility = Visibility.Collapsed;
            }

            if (ShowTargetBox && TargetW > 0 && TargetH > 0 && _targetBoxOutline != null)
            {
                _targetBoxOutline.Visibility = Visibility.Visible;
                _targetBoxOutline.Width = TargetW;
                _targetBoxOutline.Height = TargetH;
                System.Windows.Controls.Canvas.SetLeft(_targetBoxOutline, TargetX - TargetW / 2);
                System.Windows.Controls.Canvas.SetTop(_targetBoxOutline, TargetY - TargetH / 2);
            }
            else if (_targetBoxOutline != null)
            {
                _targetBoxOutline.Visibility = Visibility.Collapsed;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
            SetLayeredWindowAttributes(hwnd, 0, 1, LWA_ALPHA);
        }
    }
}
