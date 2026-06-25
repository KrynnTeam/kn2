namespace ShadowCheat.Class.Features
{
    public class VisibilityAimLock : FeatureBase
    {
        public override string Name => "Visibility Aim Lock";
        public float MinContrast { get; set; } = 30f;

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame) return;
            if (state.BestTarget == null || !state.TargetVisible) return;

            if (state.TargetContrast < MinContrast)
            {
                BlockTarget(state);
                return;
            }

            if (IsSmoke(state.BestTarget.CenterX, state.BestTarget.CenterY))
            {
                BlockTarget(state);
                return;
            }
        }

        private static void BlockTarget(GameState state)
        {
            state.BestTarget = null;
            state.TargetVisible = false;
        }

        private static bool IsSmoke(float centerX, float centerY)
        {
            int x = (int)centerX;
            int y = (int)centerY;
            using var bmp = ScreenCapture.CaptureRegion(x - 3, y - 3, 7, 7);
            if (bmp == null) return false;

            float totalSat = 0;
            int count = 0;

            for (int dy = 0; dy < 7; dy += 2)
            {
                for (int dx = 0; dx < 7; dx += 2)
                {
                    var px = bmp.GetPixel(dx, dy);
                    float max = System.Math.Max(px.R, System.Math.Max(px.G, px.B));
                    float min = System.Math.Min(px.R, System.Math.Min(px.G, px.B));
                    float sat = max < 1f ? 0f : (max - min) / max * 100f;
                    totalSat += sat;
                    count++;
                }
            }

            float avgSat = count > 0 ? totalSat / count : 0f;
            return avgSat < 15f;
        }
    }
}