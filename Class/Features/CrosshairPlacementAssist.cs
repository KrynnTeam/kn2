namespace ShadowCheat.Class.Features
{
    public class CrosshairPlacementAssist : FeatureBase
    {
        public override string Name => "Crosshair Placement Assist";
        public float DragStrength { get; set; } = 0.15f;
        public float ActivationRadius { get; set; } = 50f;
        public bool UseMouseEvent { get; set; } = true;

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame || !state.TargetVisible) return;
            if (state.TargetAngle > ActivationRadius) return;
            if (state.AimTarget == default) return;

            float screenCenterX = (float)System.Windows.SystemParameters.PrimaryScreenWidth / 2;
            float screenCenterY = (float)System.Windows.SystemParameters.PrimaryScreenHeight / 2;

            float deltaX = state.AimTarget.X - screenCenterX;
            float deltaY = state.AimTarget.Y - screenCenterY;
            float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (distance > ActivationRadius || distance < 5) return;

            float pullX = deltaX * DragStrength;
            float pullY = deltaY * DragStrength;

            pullX = Math.Clamp(pullX, -2f, 2f);
            pullY = Math.Clamp(pullY, -2f, 2f);

            if (Math.Abs(pullX) > 0.1f || Math.Abs(pullY) > 0.1f)
                InputSimulator.MoveMouse((int)(pullX * 10), (int)(pullY * 10));
        }
    }
}
