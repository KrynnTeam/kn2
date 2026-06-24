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
            {
                _lastStopTime = DateTime.UtcNow;
            }

            if (!state.IsMoving)
            {
                int elapsedMs = (int)(DateTime.UtcNow - _lastStopTime).TotalMilliseconds;
                if (elapsedMs < RecoveryBoostMs)
                {
                    float recoveryFactor = 1f - (float)elapsedMs / RecoveryBoostMs;
                    float stabX = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.15f * recoveryFactor * (1f - AccuracyMultiplier);
                    float stabY = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.2f * recoveryFactor * (1f - AccuracyMultiplier);

                    if (Math.Abs(stabX) > 0.1f || Math.Abs(stabY) > 0.1f)
                        InputSimulator.MoveMouse((int)(stabX * 10), (int)(stabY * 10));

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
