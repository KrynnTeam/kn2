namespace ShadowCheat.Class.Features
{
    public class StandstillAccuracy : FeatureBase
    {
        public override string Name => "Standstill Accuracy";
        public int RecoveryBoostMs { get; set; } = 60;
        public float AccuracyMultiplier { get; set; } = 0.7f;
        private DateTime _lastStopTime;
        private bool _wasMoving;

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame) return;

            if (!state.IsMoving && _wasMoving)
            {
                _lastStopTime = DateTime.UtcNow;
                ApplyAccuracyBoost(state);
            }

            if (!state.IsMoving && state.AccuracyRecovery > 0)
            {
                int elapsedMs = (int)(DateTime.UtcNow - _lastStopTime).TotalMilliseconds;
                if (elapsedMs < RecoveryBoostMs)
                {
                    state.AccuracyRecovery *= AccuracyMultiplier;
                }
            }

            _wasMoving = state.IsMoving;
        }

        private void ApplyAccuracyBoost(GameState state)
        {
            string procName = "cs2";
            if (GameMemory.Attach(procName))
            {
                try
                {
                    // CS2 offset: m_accuracySpread + extra recovery boost via memory write
                    IntPtr accuracyAddr = GameMemory.GetAddress(0x12345678); // placeholder
                    float currentSpread = GameMemory.Read<float>(accuracyAddr);
                    float boostedSpread = currentSpread * AccuracyMultiplier;
                    GameMemory.Write(accuracyAddr, boostedSpread);
                }
                catch { }
                finally { GameMemory.Detach(); }
            }
        }
    }
}
