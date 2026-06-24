using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ShadowCheat.Class
{
    public static class ScreenCapture
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int w, int h, IntPtr hdcSrc, int xSrc, int ySrc, int rop);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private const int SRCCOPY = 0x00CC0020;

        public static Bitmap? CaptureScreen()
        {
            int screenX = (int)System.Windows.SystemParameters.PrimaryScreenWidth;
            int screenY = (int)System.Windows.SystemParameters.PrimaryScreenHeight;

            IntPtr hdcSrc = GetDC(IntPtr.Zero);
            IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, screenX, screenY);
            if (hBitmap == IntPtr.Zero) return null;

            SelectObject(hdcDest, hBitmap);
            BitBlt(hdcDest, 0, 0, screenX, screenY, hdcSrc, 0, 0, SRCCOPY);

            Bitmap bitmap = Image.FromHbitmap(hBitmap);
            DeleteObject(hBitmap);
            DeleteDC(hdcDest);
            ReleaseDC(IntPtr.Zero, hdcSrc);
            return bitmap;
        }
    }
}
