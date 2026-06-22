namespace ShadowCheat.Class.Features
{
    public class VisibilityAimLock : FeatureBase
    {
        public override string Name => "Visibility Aim Lock";
        public float MinContrast { get; set; } = 30f;
        public bool BlockAimInSmoke { get; set; } = true;

        public override void Update(GameState state)
        {
            if (!Enabled || !state.InGame || !state.TargetVisible) return;

            if (BlockAimInSmoke && state.TargetContrast < MinContrast)
            {
                state.TargetVisible = false;
            }
        }

        public bool CanAim(GameState state)
        {
            if (!Enabled) return true;
            if (!state.TargetVisible) return false;
            if (BlockAimInSmoke && state.TargetContrast < MinContrast) return false;
            return true;
        }
    }
}
