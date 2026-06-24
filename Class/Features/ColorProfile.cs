namespace ShadowCheat.Class.Features
{
    public enum DetectionMode
    {
        Color,
        Crosshair
    }

    public class ColorProfile
    {
        public DetectionMode Mode = DetectionMode.Color;
        public byte TargetR = 255;
        public byte TargetG = 50;
        public byte TargetB = 50;
        public byte Tolerance = 60;
        public float MinConfidence = 0.3f;
        public float MinContrast = 20f;
        public int ScanRadius = 150;
        public int ScanStride = 3;
        public int MinClusterSize = 8;
        public int MaxClusterSize = 200;
        public bool UseFrameDifferencing = true;
        public float ColorWeight = 0.7f;
        public float MotionWeight = 0.3f;

        public byte CrosshairTolerance = 40;
        internal System.Drawing.Color? BaselineColor;
        internal int BaselineFrames;
        internal const int BaselineSampleFrames = 60;

        public bool MatchColor(byte r, byte g, byte b)
        {
            int dr = Math.Abs(r - TargetR);
            int dg = Math.Abs(g - TargetG);
            int db = Math.Abs(b - TargetB);
            return (dr + dg + db) / 3 <= Tolerance;
        }

        public float ColorSimilarity(byte r, byte g, byte b)
        {
            int dr = Math.Abs(r - TargetR);
            int dg = Math.Abs(g - TargetG);
            int db = Math.Abs(b - TargetB);
            float dist = (float)Math.Sqrt(dr * dr + dg * dg + db * db);
            float maxDist = (float)Math.Sqrt(3 * 255 * 255);
            return 1f - (dist / maxDist);
        }

        public bool IsCrosshairColorDifferent(System.Drawing.Color current)
        {
            if (BaselineColor == null) return false;
            int dr = Math.Abs(current.R - BaselineColor.Value.R);
            int dg = Math.Abs(current.G - BaselineColor.Value.G);
            int db = Math.Abs(current.B - BaselineColor.Value.B);
            return (dr + dg + db) / 3 > CrosshairTolerance;
        }

        public void ResetBaseline()
        {
            BaselineColor = null;
            BaselineFrames = 0;
        }
    }
}
