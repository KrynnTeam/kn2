namespace ShadowCheat.Class.Features
{
    public class NoRecoilNoise : FeatureBase
    {
        public override string Name => "No-Recoil Noise";
        public float CompensationStrength { get; set; } = 0.85f;
        public float NoiseAmplitude { get; set; } = 0.15f;
        private readonly Random _rng = new();
        private bool _wasShooting;
        private int _consecutiveShots;
        private float _accumPitch;
        private float _accumYaw;

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame) return;

            if (state.IsShooting)
            {
                _consecutiveShots++;

                float bulletNoise = 1f + (float)(_rng.NextDouble() * 2.0 - 1.0) * NoiseAmplitude;
                float patternScale = 1f + _consecutiveShots * 0.04f;

                float compensationPitch = 0.15f * CompensationStrength * bulletNoise * patternScale;
                float compensationYaw = (_consecutiveShots % 3 == 0 ? 0.08f : 0f) * CompensationStrength * bulletNoise;

                float noiseX = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.3f;
                float noiseY = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.3f;

                _accumPitch += compensationPitch;
                _accumYaw += compensationYaw;

                int moveY = -(int)(compensationPitch * 15 + noiseY);
                int moveX = -(int)(compensationYaw * 15 + noiseX);

                InputSimulator.MoveMouse(moveX, moveY);
            }
            else
            {
                _consecutiveShots = 0;
            }

            _wasShooting = state.IsShooting;
        }

        public override void Initialize()
        {
            _wasShooting = false;
            _consecutiveShots = 0;
            _accumPitch = 0;
            _accumYaw = 0;
        }
    }
}
