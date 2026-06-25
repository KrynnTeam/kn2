namespace ShadowCheat.Class.Features
{
    public class ShotOverride : FeatureBase
    {
        public override string Name => "Shot Override";
        public float TriggerRadius { get; set; } = 30f;
        public int TriggerDelayMs { get; set; } = 5;
        public bool RequirePeekDetection { get; set; } = false;
        private DateTime _lastShotTime;
        private bool _hadTargetLastFrame;
        private int _confirmCount;
        private readonly Random _rng = new();

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame) return;

            if (state.IsShooting)
            {
                _hadTargetLastFrame = state.TargetVisible && state.BestTarget != null;
                return;
            }

            if (!state.TargetVisible || state.BestTarget == null)
            {
                _hadTargetLastFrame = false;
                _confirmCount = 0;
                return;
            }

            float screenCenterX = state.CrosshairPosition.X;
            float screenCenterY = state.CrosshairPosition.Y;
            float deltaX = state.BestTarget.CenterX - screenCenterX;
            float deltaY = state.BestTarget.CenterY - screenCenterY;
            float dist = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (dist > TriggerRadius)
            {
                _hadTargetLastFrame = false;
                _confirmCount = 0;
                return;
            }

            if (RequirePeekDetection && _hadTargetLastFrame)
            {
                _confirmCount = 0;
                _hadTargetLastFrame = true;
                return;
            }

            if ((DateTime.UtcNow - _lastShotTime).TotalMilliseconds < 150) return;

            _confirmCount++;
            if (_confirmCount < 2) return;

            int delay = TriggerDelayMs + _rng.Next(25, 75);
            if (delay < 0) delay = 0;
            if (delay > 0) System.Threading.Thread.Sleep(delay);
            InputSimulator.Click();
            _lastShotTime = DateTime.UtcNow;
            _confirmCount = 0;

            _hadTargetLastFrame = true;
        }

        public override void Initialize()
        {
            _hadTargetLastFrame = false;
            _confirmCount = 0;
        }
    }
}