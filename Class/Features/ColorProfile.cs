namespace ShadowCheat.Class.Features
{
    public class ColorProfile
    {
        public byte TargetR = 200;
        public byte TargetG = 40;
        public byte TargetB = 40;
        public byte Tolerance = 120;
        public float MinConfidence = 0.1f;
        public float MinContrast = 3f;
        public int ScanRadius = 200;
        public int ScanStride = 2;
        public int MinClusterSize = 2;
        public int MaxClusterSize = 500;

        public bool MatchColor(byte r, byte g, byte b)
        {
            int dr = Math.Abs(r - TargetR);
            int dg = Math.Abs(g - TargetG);
            int db = Math.Abs(b - TargetB);
            return dr + dg + db <= Tolerance * 3;
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
