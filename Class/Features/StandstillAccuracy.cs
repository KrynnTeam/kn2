namespace ShadowCheat.Class.Features
{
    public class StandstillAccuracy : FeatureBase
    {
        public override string Name => "Standstill Accuracy";
        public int RecoveryBoostMs { get; set; } = 60;
        public float AccuracyMultiplier { get; set; } = 0.7f;
        private DateTime _lastStopTime;
        private bool _wasMoving;
        private readonly Random _rng = new();

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame) return;

            if (!state.IsMoving && _wasMoving)
                _lastStopTime = DateTime.UtcNow;

            if (!state.IsMoving)
            {
                int elapsedMs = (int)(DateTime.UtcNow - _lastStopTime).TotalMilliseconds;

                if (elapsedMs < RecoveryBoostMs)
                {
                    float recoveryFactor = 1f - (float)elapsedMs / RecoveryBoostMs;

                    if (state.TargetVisible && state.BestTarget != null)
                    {
                        float dx = state.BestTarget.CenterX - state.CrosshairPosition.X;
                        float dy = state.BestTarget.CenterY - state.CrosshairPosition.Y;
                        float pullStrength = 0.15f * recoveryFactor * (1f - AccuracyMultiplier);
                        float pullX = dx * pullStrength;
                        float pullY = dy * pullStrength;
                        pullX = Math.Clamp(pullX, -2f, 2f);
                        pullY = Math.Clamp(pullY, -2f, 2f);

                        if (Math.Abs(pullX) > 0.1f || Math.Abs(pullY) > 0.1f)
                            InputSimulator.MoveMouse((int)(pullX * 10), (int)(pullY * 10));
                    }
                    else
                    {
                        float jitterX = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.05f * recoveryFactor;
                        float jitterY = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.05f * recoveryFactor;

                        if (Math.Abs(jitterX) > 0.05f || Math.Abs(jitterY) > 0.05f)
                            InputSimulator.MoveMouse((int)(jitterX * 10), (int)(jitterY * 10));
                    }

                    state.AccuracyRecovery = 1f - recoveryFactor * (1f - AccuracyMultiplier);
                }
                else
                {
                    state.AccuracyRecovery = 1f;
                }
            }
            else
            {
                state.AccuracyRecovery = 0f;
            }

            _wasMoving = state.IsMoving;
        }
    }
}