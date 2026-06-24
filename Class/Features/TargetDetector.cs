using System.Drawing;
using System.Drawing.Imaging;

namespace ShadowCheat.Class.Features
{
    public class TargetDetector
    {
        public ColorProfile Profile = new();
        private Bitmap? _lastFrame;
        private readonly object _lock = new();

        public void SetColor(byte r, byte g, byte b)
        {
            Profile.TargetR = r;
            Profile.TargetG = g;
            Profile.TargetB = b;
        }

        public List<DetectedTarget> Detect(float screenCenterX, float screenCenterY)
        {
            var results = new List<DetectedTarget>();
            int radius = Profile.ScanRadius;
            int stride = Profile.ScanStride;

            int captureX = Math.Max(0, (int)screenCenterX - radius);
            int captureY = Math.Max(0, (int)screenCenterY - radius);
            int captureW = Math.Min(radius * 2, (int)System.Windows.SystemParameters.PrimaryScreenWidth - captureX);
            int captureH = Math.Min(radius * 2, (int)System.Windows.SystemParameters.PrimaryScreenHeight - captureY);

            if (captureW <= 0 || captureH <= 0) return results;

            using var bmp = CaptureRegion(captureX, captureY, captureW, captureH);
            if (bmp == null) return results;

            var clusters = FindColorClusters(bmp, captureX, captureY, screenCenterX, screenCenterY);

            foreach (var (cx, cy, w, h, confidence, contrast) in clusters)
            {
                float dx = cx - screenCenterX;
                float dy = cy - screenCenterY;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                results.Add(new DetectedTarget
                {
                    CenterX = cx,
                    CenterY = cy,
                    Width = w,
                    Height = h,
                    Confidence = confidence,
                    Contrast = contrast,
                    DistanceFromCrosshair = dist
                });
            }

            lock (_lock)
            {
                _lastFrame?.Dispose();
                _lastFrame = (Bitmap)bmp.Clone();
            }

            return results;
        }

        public DetectedTarget? DetectCrosshair(float screenCenterX, float screenCenterY)
        {
            int x = Math.Max(0, (int)screenCenterX);
            int y = Math.Max(0, (int)screenCenterY);

            using var bmp = CaptureRegion(x, y, 1, 1);
            if (bmp == null) return null;

            var pixel = bmp.GetPixel(0, 0);

            if (Profile.BaselineFrames < ColorProfile.BaselineSampleFrames)
            {
                Profile.BaselineFrames++;
                if (Profile.BaselineColor == null)
                    Profile.BaselineColor = pixel;
                else
                {
                    int br = (Profile.BaselineColor.Value.R * (Profile.BaselineFrames - 1) + pixel.R) / Profile.BaselineFrames;
                    int bg = (Profile.BaselineColor.Value.G * (Profile.BaselineFrames - 1) + pixel.G) / Profile.BaselineFrames;
                    int bb = (Profile.BaselineColor.Value.B * (Profile.BaselineFrames - 1) + pixel.B) / Profile.BaselineFrames;
                    Profile.BaselineColor = Color.FromArgb(br, bg, bb);
                }
                return null;
            }

            if (!Profile.IsCrosshairColorDifferent(pixel)) return null;

            // Center pixel changed — scan a tight ring around center for a target body
            int scanR = 40;
            int captureX = Math.Max(0, (int)screenCenterX - scanR);
            int captureY = Math.Max(0, (int)screenCenterY - scanR);
            int captureW = Math.Min(scanR * 2, (int)System.Windows.SystemParameters.PrimaryScreenWidth - captureX);
            int captureH = Math.Min(scanR * 2, (int)System.Windows.SystemParameters.PrimaryScreenHeight - captureY);

            using var ring = CaptureRegion(captureX, captureY, captureW, captureH);
            if (ring != null)
            {
                int midX = (int)screenCenterX - captureX;
                int midY = (int)screenCenterY - captureY;
                int searchRadius = 35;
                var cluster = FindNearestCluster(ring, captureX, captureY, midX, midY, searchRadius);
                if (cluster != null)
                    return cluster;
            }

            // Fallback: return center with distance = scanR so aim assist pulls inward
            return new DetectedTarget
            {
                CenterX = screenCenterX + 10,
                CenterY = screenCenterY + 10,
                Width = 10, Height = 10,
                Confidence = 0.8f,
                Contrast = 80f,
                DistanceFromCrosshair = 15
            };
        }

        private DetectedTarget? FindNearestCluster(Bitmap bmp, int offsetX, int offsetY, int centerX, int centerY, int radius)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            float bestDist = float.MaxValue;
            DetectedTarget? best = null;

            for (int y = 0; y < h; y += 2)
            {
                for (int x = 0; x < w; x += 2)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float dist = dx * dx + dy * dy;
                    if (dist > radius * radius) continue;

                    var px = bmp.GetPixel(x, y);
                    if (!Profile.MatchColor(px.R, px.G, px.B)) continue;

                    float worldX = x + offsetX;
                    float worldY = y + offsetY;
                    float dFromCrosshair = MathF.Sqrt(dx * dx + dy * dy);
                    if (dFromCrosshair < bestDist)
                    {
                        bestDist = dFromCrosshair;
                        best = new DetectedTarget
                        {
                            CenterX = worldX, CenterY = worldY,
                            Width = 8, Height = 8,
                            Confidence = 0.9f, Contrast = 70f,
                            DistanceFromCrosshair = dFromCrosshair
                        };
                    }
                }
            }

            return best;
        }

        private Bitmap? CaptureRegion(int x, int y, int w, int h)
        {
            IntPtr hdcSrc = NativeMethods.GetDC(IntPtr.Zero);
            IntPtr hdcDest = NativeMethods.CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = NativeMethods.CreateCompatibleBitmap(hdcSrc, w, h);
            if (hBitmap == IntPtr.Zero) return null;

            NativeMethods.SelectObject(hdcDest, hBitmap);
            NativeMethods.BitBlt(hdcDest, 0, 0, w, h, hdcSrc, x, y, NativeMethods.SRCCOPY);

            Bitmap bitmap;
            try { bitmap = Image.FromHbitmap(hBitmap); }
            catch { bitmap = new Bitmap(w, h); }

            NativeMethods.DeleteObject(hBitmap);
            NativeMethods.DeleteDC(hdcDest);
            NativeMethods.ReleaseDC(IntPtr.Zero, hdcSrc);
            return bitmap;
        }

        private List<(float cx, float cy, float w, float h, float conf, float contrast)> FindColorClusters(
            Bitmap bmp, int offsetX, int offsetY, float screenCenterX, float screenCenterY)
        {
            var clusterCenters = new List<(float cx, float cy, float w, float h, float conf, float contrast)>();
            int w = bmp.Width;
            int h = bmp.Height;
            int stride = Profile.ScanStride;

            var visited = new bool[w, h];
            var tempCluster = new List<(int x, int y, byte r, byte g, byte b)>();

            for (int y = 0; y < h; y += stride)
            {
                for (int x = 0; x < w; x += stride)
                {
                    if (visited[x, y]) continue;
                    var pixel = bmp.GetPixel(x, y);
                    if (!Profile.MatchColor(pixel.R, pixel.G, pixel.B)) continue;

                    tempCluster.Clear();
                    FloodFind(bmp, x, y, visited, tempCluster, w, h);

                    if (tempCluster.Count < Profile.MinClusterSize) continue;
                    if (tempCluster.Count > Profile.MaxClusterSize) continue;

                    float sumX = 0, sumY = 0;
                    float minX = float.MaxValue, maxX = float.MinValue;
                    float minY = float.MaxValue, maxY = float.MinValue;
                    float totalSim = 0;

                    foreach (var (px, py, pr, pg, pb) in tempCluster)
                    {
                        sumX += px;
                        sumY += py;
                        totalSim += Profile.ColorSimilarity(pr, pg, pb);
                        if (px < minX) minX = px; if (px > maxX) maxX = px;
                        if (py < minY) minY = py; if (py > maxY) maxY = py;
                    }

                    float avgX = sumX / tempCluster.Count + offsetX;
                    float avgY = sumY / tempCluster.Count + offsetY;
                    float avgConf = totalSim / tempCluster.Count;
                    float clusterW = maxX - minX;
                    float clusterH = maxY - minY;

                    float contrast = ScreenCapture.GetPixelContrast(bmp,
                        (int)((minX + maxX) / 2), (int)((minY + maxY) / 2), 8);

                    clusterCenters.Add((avgX, avgY, clusterW, clusterH, avgConf, contrast));
                }
            }

            return clusterCenters;
        }

        private void FloodFind(Bitmap bmp, int startX, int startY, bool[,] visited,
            List<(int x, int y, byte r, byte g, byte b)> cluster, int w, int h)
        {
            var stack = new Stack<(int x, int y)>();
            stack.Push((startX, startY));
            int maxCluster = Profile.MaxClusterSize;

            while (stack.Count > 0 && cluster.Count < maxCluster)
            {
                var (cx, cy) = stack.Pop();
                if (cx < 0 || cx >= w || cy < 0 || cy >= h) continue;
                if (visited[cx, cy]) continue;
                visited[cx, cy] = true;

                var pixel = bmp.GetPixel(cx, cy);
                if (!Profile.MatchColor(pixel.R, pixel.G, pixel.B)) continue;

                cluster.Add((cx, cy, pixel.R, pixel.G, pixel.B));

                stack.Push((cx + 1, cy));
                stack.Push((cx - 1, cy));
                stack.Push((cx, cy + 1));
                stack.Push((cx, cy - 1));
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _lastFrame?.Dispose();
                _lastFrame = null;
            }
        }

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern IntPtr GetDC(IntPtr hWnd);
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int w, int h, IntPtr hdcSrc, int xSrc, int ySrc, int rop);
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hdc);
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            public const int SRCCOPY = 0x00CC0020;
        }
    }
}
