namespace ShadowCheat.Class.Features
{
    public class RapidFire : FeatureBase
    {
        public override string Name => "Rapid Fire";
        public int CpsLimit { get; set; } = 12;
        private DateTime _lastShotTime;
        private readonly Random _rng = new();

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame) return;
            if (!InputSimulator.IsKeyDown(0x01)) return;

            int delayMs = 1000 / CpsLimit;
            delayMs += _rng.Next(-2, 3);
            if (delayMs < 10) delayMs = 10;

            if ((DateTime.UtcNow - _lastShotTime).TotalMilliseconds < delayMs) return;

            InputSimulator.Click();
            _lastShotTime = DateTime.UtcNow;
        }
    }
}
