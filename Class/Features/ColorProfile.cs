namespace ShadowCheat.Class.Features
{
    public class ColorProfile
    {
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
    }
}
