namespace ShadowCheat.Class.Features
{
    public class TriggerBot : FeatureBase
    {
        public override string Name => "Trigger Bot";
        public override bool RequiresScreenDetection => true;
        public float TriggerRadius { get; set; } = 100f;
        public int DelayMs { get; set; } = 5;
        public bool RequireAimKey { get; set; } = false;
        public bool HeadshotOnly { get; set; } = false;
        public float HeadshotZonePct { get; set; } = 35f;
        public int ConfirmationFrames { get; set; } = 1;
        public bool RequireMovement { get; set; } = false;
        private DateTime _lastShotTime;
        private readonly Random _rng = new();
        private int _confirmCount;
        private float _lastTargetX;
        private float _lastTargetY;
        private bool _hadTarget;

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame || !state.TargetVisible) return;
            if (RequireAimKey && !state.AimKeyHeld) return;
            if (state.BestTarget == null) return;
            if ((DateTime.UtcNow - _lastShotTime).TotalMilliseconds < 50) return;

            float dist = state.TargetDistance;
            if (dist > TriggerRadius)
            {
                ResetConfirmation();
                return;
            }

            float crosshairY = state.CrosshairPosition.Y;
            float targetCY = state.BestTarget.CenterY;
            float targetH = state.BestTarget.Height;

            if (HeadshotOnly && targetH > 0)
            {
                float headTop = targetCY - targetH / 2;
                float headBottom = headTop + targetH * (HeadshotZonePct / 100f);
                if (crosshairY < headTop || crosshairY > headBottom)
                {
                    ResetConfirmation();
                    return;
                }
            }

            if (RequireMovement && _hadTarget)
            {
                float dx = state.BestTarget.CenterX - _lastTargetX;
                float dy = state.BestTarget.CenterY - _lastTargetY;
                float moveDist = (float)Math.Sqrt(dx * dx + dy * dy);
                if (moveDist < 1f)
                {
                    ResetConfirmation();
                    return;
                }
            }

            _lastTargetX = state.BestTarget.CenterX;
            _lastTargetY = state.BestTarget.CenterY;
            _hadTarget = true;

            _confirmCount++;
            if (_confirmCount < ConfirmationFrames) return;

            int delay = DelayMs + _rng.Next(-3, 4);
            if (delay < 0) delay = 0;
            if (delay > 0) System.Threading.Thread.Sleep(delay);
            InputSimulator.Click();
            _lastShotTime = DateTime.UtcNow;
            ResetConfirmation();
        }

        private void ResetConfirmation() => _confirmCount = 0;

        public override void Shutdown()
        {
            ResetConfirmation();
            _hadTarget = false;
        }
    }
}
