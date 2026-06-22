namespace ShadowCheat.Class.Features
{
    public class FlickAssist : FeatureBase
    {
        public override string Name => "Flick Assist";
        public float ShortFlickThreshold { get; set; } = 30f;
        public float LongFlickThreshold { get; set; } = 60f;
        public float ErrorRange { get; set; } = 0.20f;
        public float AssistStrength { get; set; } = 0.6f;
        private readonly Random _rng = new();
        private bool _wasFlicking;

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame || !state.TargetVisible) return;
            if (!state.FlickDetected) return;

            if (state.FlickAngle < ShortFlickThreshold) return;

            if (state.FlickAngle >= LongFlickThreshold)
            {
                float screenCenterX = (float)System.Windows.SystemParameters.PrimaryScreenWidth / 2;
                float screenCenterY = (float)System.Windows.SystemParameters.PrimaryScreenHeight / 2;
                float deltaX = state.AimTarget.X - screenCenterX;
                float deltaY = state.AimTarget.Y - screenCenterY;

                float error = (float)(_rng.NextDouble() * 2.0 - 1.0) * ErrorRange;
                float assistX = deltaX * AssistStrength * (1 + error);
                float assistY = deltaY * AssistStrength * (1 + error);

                assistX = Math.Clamp(assistX, -20f, 20f);
                assistY = Math.Clamp(assistY, -20f, 20f);

                if (Math.Abs(assistX) > 0.5f || Math.Abs(assistY) > 0.5f)
                    InputSimulator.MoveMouse((int)assistX, (int)assistY);
            }

            _wasFlicking = true;
        }
    }
}
