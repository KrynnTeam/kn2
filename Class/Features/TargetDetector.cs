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

        public (byte r, byte g, byte b)? SampleColorAtCrosshair()
        {
            float cx = (float)System.Windows.SystemParameters.PrimaryScreenWidth / 2;
            float cy = (float)System.Windows.SystemParameters.PrimaryScreenHeight / 2;
            int x = (int)cx;
            int y = (int)cy;
            using var bmp = CaptureRegion(x - 2, y - 2, 5, 5);
            if (bmp == null) return null;
            var data = LockBitmap(bmp);
            if (data == null) return null;
            try
            {
                int stride = data.Stride;
                IntPtr ptr = data.Scan0;
                byte[] bytes = new byte[stride * bmp.Height];
                System.Runtime.InteropServices.Marshal.Copy(ptr, bytes, 0, bytes.Length);

                long sumR = 0, sumG = 0, sumB = 0;
                int count = 0;
                for (int py = 0; py < bmp.Height; py++)
                {
                    int row = py * stride;
                    for (int px = 0; px < bmp.Width; px++)
                    {
                        int idx = row + px * 4;
                        sumB += bytes[idx];
                        sumG += bytes[idx + 1];
                        sumR += bytes[idx + 2];
                        count++;
                    }
                }
                if (count == 0) return null;
                return ((byte)(sumR / count), (byte)(sumG / count), (byte)(sumB / count));
            }
            finally { bmp.UnlockBits(data); }
        }

        public List<DetectedTarget> Detect(float screenCenterX, float screenCenterY)
        {
            var results = new List<DetectedTarget>();
            int radius = Profile.ScanRadius;

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

        private static BitmapData? LockBitmap(Bitmap bmp)
        {
            try
            {
                return bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);
            }
            catch { return null; }
        }

        private static byte GetB(byte[] bytes, int stride, int x, int y) =>
            bytes[y * stride + x * 4];
        private static byte GetG(byte[] bytes, int stride, int x, int y) =>
            bytes[y * stride + x * 4 + 1];
        private static byte GetR(byte[] bytes, int stride, int x, int y) =>
            bytes[y * stride + x * 4 + 2];

        private List<(float cx, float cy, float w, float h, float conf, float contrast)> FindColorClusters(
            Bitmap bmp, int offsetX, int offsetY, float screenCenterX, float screenCenterY)
        {
            var result = new List<(float, float, float, float, float, float)>();
            var data = LockBitmap(bmp);
            if (data == null) return result;

            try
            {
                int w = bmp.Width;
                int h = bmp.Height;
                int stride = data.Stride;
                IntPtr ptr = data.Scan0;
                byte[] bytes = new byte[stride * h];
                System.Runtime.InteropServices.Marshal.Copy(ptr, bytes, 0, bytes.Length);

                int scanStride = Profile.ScanStride;
                var visited = new bool[w, h];
                var stack = new Stack<(int x, int y)>();
                var cluster = new List<(int x, int y)>();

                for (int y = 0; y < h; y += scanStride)
                {
                    for (int x = 0; x < w; x += scanStride)
                    {
                        if (visited[x, y]) continue;
                        byte pr = GetR(bytes, stride, x, y);
                        byte pg = GetG(bytes, stride, x, y);
                        byte pb = GetB(bytes, stride, x, y);
                        if (!Profile.MatchColor(pr, pg, pb)) continue;

                        cluster.Clear();
                        stack.Clear();
                        stack.Push((x, y));

                        while (stack.Count > 0 && cluster.Count < Profile.MaxClusterSize)
                        {
                            var (cx, cy) = stack.Pop();
                            if (cx < 0 || cx >= w || cy < 0 || cy >= h) continue;
                            if (visited[cx, cy]) continue;
                            visited[cx, cy] = true;

                            byte cr = GetR(bytes, stride, cx, cy);
                            byte cg = GetG(bytes, stride, cx, cy);
                            byte cb = GetB(bytes, stride, cx, cy);
                            if (!Profile.MatchColor(cr, cg, cb)) continue;

                            cluster.Add((cx, cy));

                            stack.Push((cx + 1, cy));
                            stack.Push((cx - 1, cy));
                            stack.Push((cx, cy + 1));
                            stack.Push((cx, cy - 1));
                        }

                        if (cluster.Count < Profile.MinClusterSize) continue;

                        float sumX = 0, sumY = 0;
                        float minX = float.MaxValue, maxX = float.MinValue;
                        float minY = float.MaxValue, maxY = float.MinValue;
                        float totalSim = 0;

                        foreach (var (px, py) in cluster)
                        {
                            sumX += px;
                            sumY += py;
                            totalSim += Profile.ColorSimilarity(
                                GetR(bytes, stride, px, py),
                                GetG(bytes, stride, px, py),
                                GetB(bytes, stride, px, py));
                            if (px < minX) minX = px; if (px > maxX) maxX = px;
                            if (py < minY) minY = py; if (py > maxY) maxY = py;
                        }

                        float avgX = sumX / cluster.Count + offsetX;
                        float avgY = sumY / cluster.Count + offsetY;
                        float avgConf = totalSim / cluster.Count;
                        float clusterW = maxX - minX;
                        float clusterH = maxY - minY;

                        float contrast = CalcContrast(bytes, stride, w, h,
                            (int)((minX + maxX) / 2), (int)((minY + maxY) / 2), 8);

                        result.Add((avgX, avgY, clusterW, clusterH, avgConf, contrast));
                    }
                }

                return result;
            }
            finally { bmp.UnlockBits(data); }
        }

        private static float CalcContrast(byte[] bytes, int stride, int bw, int bh, int cx, int cy, int size)
        {
            int half = size / 2;
            long totalR = 0, totalG = 0, totalB = 0;
            int count = 0;

            for (int dy = -half; dy <= half; dy++)
            {
                for (int dx = -half; dx <= half; dx++)
                {
                    int px = Math.Clamp(cx + dx, 0, bw - 1);
                    int py = Math.Clamp(cy + dy, 0, bh - 1);
                    totalR += GetR(bytes, stride, px, py);
                    totalG += GetG(bytes, stride, px, py);
                    totalB += GetB(bytes, stride, px, py);
                    count++;
                }
            }

            float avgR = totalR / (float)count;
            float avgG = totalG / (float)count;
            float avgB = totalB / (float)count;

            float variance = 0;
            for (int dy = -half; dy <= half; dy++)
            {
                for (int dx = -half; dx <= half; dx++)
                {
                    int px = Math.Clamp(cx + dx, 0, bw - 1);
                    int py = Math.Clamp(cy + dy, 0, bh - 1);
                    byte r = GetR(bytes, stride, px, py);
                    byte g = GetG(bytes, stride, px, py);
                    byte b = GetB(bytes, stride, px, py);
                    variance += (r - avgR) * (r - avgR) +
                                (g - avgG) * (g - avgG) +
                                (b - avgB) * (b - avgB);
                }
            }
            variance /= count;
            return (float)Math.Sqrt(variance / 3);
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
