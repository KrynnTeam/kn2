namespace ShadowCheat.Class.Features
{
    public class ShotOverride : FeatureBase
    {
        public override string Name => "Shot Override";
        public float TriggerRadius { get; set; } = 30f;
        public int TriggerDelayMs { get; set; } = 5;
        public bool RequirePeekDetection { get; set; } = false;
        private DateTime _lastShotTime;

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame || !state.TargetVisible) return;

            if (RequirePeekDetection && !state.EnemyAboutToPeek) return;

            float screenCenterX = (float)System.Windows.SystemParameters.PrimaryScreenWidth / 2;
            float screenCenterY = (float)System.Windows.SystemParameters.PrimaryScreenHeight / 2;
            float deltaX = state.AimTarget.X - screenCenterX;
            float deltaY = state.AimTarget.Y - screenCenterY;
            float dist = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (dist > TriggerRadius) return;
            if ((DateTime.UtcNow - _lastShotTime).TotalMilliseconds < 150) return;

            int delay = TriggerDelayMs + new Random().Next(-2, 3);
            if (delay < 0) delay = 0;
            System.Threading.Thread.Sleep(delay);
            InputSimulator.Click();
            _lastShotTime = DateTime.UtcNow;
        }
    }
}
