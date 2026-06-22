namespace ShadowCheat.Class.Features
{
    public class NoRecoilNoise : FeatureBase
    {
        public override string Name => "No-Recoil Noise";
        public float CompensationStrength { get; set; } = 0.85f;
        public float NoiseAmplitude { get; set; } = 0.15f;
        private readonly Random _rng = new();
        private int _lastShotCount;
        private float _accumPitch;
        private float _accumYaw;

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame) return;
            if (!state.IsShooting) return;
            if (state.ShotCount <= _lastShotCount) return;

            int bulletsFired = state.ShotCount - _lastShotCount;
            for (int i = 0; i < bulletsFired; i++)
            {
                float bulletNoise = 1f + (float)(_rng.NextDouble() * 2.0 - 1.0) * NoiseAmplitude;
                float compensationPitch = state.RecoilPitch * CompensationStrength * bulletNoise;
                float compensationYaw = state.RecoilYaw * CompensationStrength * bulletNoise * 0.5f;

                float noiseX = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.3f;
                float noiseY = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.3f;

                _accumPitch += compensationPitch;
                _accumYaw += compensationYaw;

                int moveY = -(int)(compensationPitch * 15 + noiseY);
                int moveX = -(int)(compensationYaw * 15 + noiseX);

                InputSimulator.MoveMouse(moveX, moveY);
            }

            _lastShotCount = state.ShotCount;
        }

        public override void Initialize()
        {
            _lastShotCount = 0;
            _accumPitch = 0;
            _accumYaw = 0;
        }
    }
}
