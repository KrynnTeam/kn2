namespace ShadowCheat.Class.Features
{
    public abstract class FeatureBase
    {
        public bool Enabled { get; set; }
        public abstract string Name { get; }
        public abstract void Update(GameState state);
        public virtual void Initialize() { }
        public virtual void Shutdown() { }
    }
}
