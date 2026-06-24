namespace ShadowCheat.Class.Features
{
    public class ColorAimAssist : FeatureBase
    {
        public override string Name => "Color Aim Assist";
        public override bool RequiresScreenDetection => true;
        public float MaxDistance { get; set; } = 400f;
        public bool RequireAimKey { get; set; } = false;

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame || !state.TargetVisible) return;
            if (state.BestTarget == null) return;
            if (RequireAimKey && !state.AimKeyHeld) return;

            float dist = state.TargetDistance;
            if (dist > MaxDistance || dist < 0.5f) return;

            var cursor = System.Windows.Forms.Cursor.Position;
            float deltaX = state.AimTarget.X - cursor.X;
            float deltaY = state.AimTarget.Y - cursor.Y;

            // Move at 1.2x speed toward target — smooth but fast
            float moveX = deltaX * 1.2f;
            float moveY = deltaY * 1.2f;

            // Clamp to ±80px/frame (4800px/s at 60fps)
            moveX = Math.Clamp(moveX, -80, 80);
            moveY = Math.Clamp(moveY, -80, 80);

            if (Math.Abs(moveX) > 0.5f || Math.Abs(moveY) > 0.5f)
                InputSimulator.MoveMouse((int)moveX, (int)moveY);
        }
    }
}
