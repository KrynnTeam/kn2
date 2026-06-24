using System.Numerics;

namespace ShadowCheat.Class.Features
{
    public class CrosshairPlacementAssist : FeatureBase
    {
        public override string Name => "Aim Assist";
        public float AimFov { get; set; } = 150f;
        public float Smoothing { get; set; } = 0.15f;
        public float Deadzone { get; set; } = 5f;
        public float DragStrength { get; set; } = 0.35f;
        public int AimKey { get; set; } = 0x02;
        public bool RequireAimKey { get; set; } = true;
        private readonly Random _rng = new();

        private DetectedTarget? _lastTarget;
        private Vector2 _lastTargetPos;
        private Vector2 _targetVelocity;
        private int _framesSinceTarget;

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame) return;

            if (RequireAimKey && !InputSimulator.IsKeyDown(AimKey)) return;

            float screenCenterX = (float)System.Windows.SystemParameters.PrimaryScreenWidth / 2;
            float screenCenterY = (float)System.Windows.SystemParameters.PrimaryScreenHeight / 2;

            if (!state.TargetVisible || state.BestTarget == null)
            {
                _framesSinceTarget++;
                return;
            }

            var target = state.BestTarget;
            float targetX = target.CenterX;
            float targetY = target.CenterY;

            if (_lastTarget != null && _framesSinceTarget < 5)
            {
                float dx = target.CenterX - _lastTargetPos.X;
                float dy = target.CenterY - _lastTargetPos.Y;
                float dist = MathF.Sqrt(dx * dx + dy * dy);

                if (dist < 100f)
                {
                    _targetVelocity.X = dx * 60f;
                    _targetVelocity.Y = dy * 60f;
                    targetX += _targetVelocity.X * 0.016f;
                    targetY += _targetVelocity.Y * 0.016f;
                }
                else
                {
                    _targetVelocity = Vector2.Zero;
                }
            }
            else
            {
                _targetVelocity = Vector2.Zero;
            }

            _lastTarget = target;
            _lastTargetPos = new Vector2(target.CenterX, target.CenterY);
            _framesSinceTarget = 0;

            float deltaX = targetX - screenCenterX;
            float deltaY = targetY - screenCenterY;
            float distance = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (distance > AimFov || distance < Deadzone) return;

            float fovProgress = distance / AimFov;
            float pullStrength = (1f - fovProgress) * DragStrength;
            pullStrength = pullStrength * (1f - MathF.Pow(1f - pullStrength, 3f));
            pullStrength = Math.Clamp(pullStrength, 0.01f, 0.6f);

            float pullX = deltaX * pullStrength * Smoothing;
            float pullY = deltaY * pullStrength * Smoothing;

            float jitterX = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.08f;
            float jitterY = (float)(_rng.NextDouble() * 2.0 - 1.0) * 0.08f;
            pullX += jitterX;
            pullY += jitterY;

            pullX = Math.Clamp(pullX, -3f, 3f);
            pullY = Math.Clamp(pullY, -3f, 3f);

            if (MathF.Abs(pullX) > 0.1f || MathF.Abs(pullY) > 0.1f)
                InputSimulator.MoveMouse((int)(pullX * 10), (int)(pullY * 10));
        }

        public override void Initialize()
        {
            _lastTarget = null;
            _targetVelocity = Vector2.Zero;
            _framesSinceTarget = 999;
        }
    }
}
