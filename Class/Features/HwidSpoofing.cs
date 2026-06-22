using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ShadowCheat.Class.Features
{
    public class HwidSpoofing : FeatureBase
    {
        public override string Name => "HWID Spoofing";
        public int RotationIntervalMinutes { get; set; } = 5;
        private DateTime _lastRotation = DateTime.MinValue;
        private string[] _classNames = { "ShadowWindow", "RenderWindow", "OverlayWindow", "HelperWindow", "WorkerWindow" };
        private string[] _windowTitles = { "Shadow", "Runtime", "Service", "Helper", "Agent" };
        private int _currentIndex;
        private IntPtr _originalClassName;
        private string _originalTitle = "";

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_APPWINDOW = 0x40000;
        private const int WS_EX_TOOLWINDOW = 0x80;

        private Window? _targetWindow;

        public void AttachToWindow(Window window)
        {
            _targetWindow = window;
        }

        public override void Update(GameState state)
        {
            if (!Enabled) return;
            if (_targetWindow == null) return;

            if ((DateTime.UtcNow - _lastRotation).TotalMinutes < RotationIntervalMinutes) return;

            RotateWindowIdentity();
            _lastRotation = DateTime.UtcNow;
        }

        private void RotateWindowIdentity()
        {
            if (_targetWindow == null) return;
            var hwnd = new WindowInteropHelper(_targetWindow).Handle;
            if (hwnd == IntPtr.Zero) return;

            _currentIndex = (_currentIndex + 1) % _classNames.Length;

            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            exStyle = (exStyle & ~WS_EX_APPWINDOW) | WS_EX_TOOLWINDOW;
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);

            string newTitle = $"{_windowTitles[_currentIndex]}_{DateTime.UtcNow.Ticks % 100000}";
            SetWindowText(hwnd, newTitle);
        }

        public override void Initialize()
        {
            _lastRotation = DateTime.UtcNow;
        }
    }
}
