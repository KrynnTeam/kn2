using System.Numerics;

namespace ShadowCheat.Class.Features
{
    public class FlickAssist : FeatureBase
    {
        public override string Name => "Flick Assist";
        public float ShortFlickThreshold { get; set; } = 30f;
        public float LongFlickThreshold { get; set; } = 60f;
        public float ErrorRange { get; set; } = 0.20f;
        public float AssistStrength { get; set; } = 0.6f;
        private readonly Random _rng = new();
        private bool _flickActive;
        private Vector2 _flickDirection;
        private Vector2 _flickStartCrosshair;
        private int _lowSpeedFrames;

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame) return;

            float speed = state.MouseDelta.Length();
            bool flicking = speed > 30f;

            if (!flicking && !_flickActive) return;

            if (flicking)
            {
                if (!_flickActive)
                {
                    _flickActive = true;
                    _flickDirection = Vector2.Normalize(state.MouseDelta);
                    _flickStartCrosshair = state.CrosshairPosition;
                }
                _lowSpeedFrames = 0;

                Vector2 currentDir = Vector2.Normalize(state.MouseDelta);
                _flickDirection = Vector2.Lerp(_flickDirection, currentDir, 0.3f);
            }
            else
            {
                _lowSpeedFrames++;
                if (_lowSpeedFrames > 5)
                {
                    _flickActive = false;
                    return;
                }
                return;
            }

            float flickAngleDeg = state.FlickAngle;
            if (flickAngleDeg < ShortFlickThreshold) return;

            if (state.DetectedTargets == null || state.DetectedTargets.Count == 0) return;

            float screenCenterX = state.CrosshairPosition.X;
            float screenCenterY = state.CrosshairPosition.Y;

            DetectedTarget? bestInCone = null;
            float bestDist = float.MaxValue;

            foreach (var t in state.DetectedTargets)
            {
                float toTargetX = t.CenterX - screenCenterX;
                float toTargetY = t.CenterY - screenCenterY;
                float toTargetLen = MathF.Sqrt(toTargetX * toTargetX + toTargetY * toTargetY);
                if (toTargetLen < 1f) continue;

                float dot = (toTargetX / toTargetLen) * _flickDirection.X +
                            (toTargetY / toTargetLen) * _flickDirection.Y;
                float angleDeg = MathF.Acos(Math.Clamp(dot, -1f, 1f)) * 57.2958f;

                if (angleDeg > 35f) continue;

                if (toTargetLen < bestDist)
                {
                    bestDist = toTargetLen;
                    bestInCone = t;
                }
            }

            if (bestInCone == null) return;

            float targetX = bestInCone.CenterX;
            float targetY = bestInCone.CenterY;

            float deltaX = targetX - screenCenterX;
            float deltaY = targetY - screenCenterY;
            float distance = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);

            float angleRatio = Math.Clamp(flickAngleDeg / LongFlickThreshold, 0f, 1f);
            float strength = AssistStrength * angleRatio;

            float error = 1f + (float)(_rng.NextDouble() * 2.0 - 1.0) * ErrorRange;
            float pullX = deltaX * strength * error;
            float pullY = deltaY * strength * error;

            pullX = Math.Clamp(pullX, -25f, 25f);
            pullY = Math.Clamp(pullY, -25f, 25f);

            if (MathF.Abs(pullX) > 0.5f || MathF.Abs(pullY) > 0.5f)
                InputSimulator.MoveMouse((int)pullX, (int)pullY);
        }

        public override void Initialize()
        {
            _flickActive = false;
            _lowSpeedFrames = 0;
            _flickDirection = Vector2.Zero;
        }
    }
}