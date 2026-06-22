namespace ShadowCheat.Class.Features
{
    public class DetectedTarget
    {
        public float CenterX;
        public float CenterY;
        public float Width;
        public float Height;
        public float Confidence;
        public float Contrast;
        public float DistanceFromCrosshair;
    }

    public class GameState
    {
        public bool InGame;
        public System.Numerics.Vector2 CrosshairPosition;
        public System.Numerics.Vector2 AimTarget;
        public float TargetDistance;
        public float TargetAngle;
        public float TargetVelocity;
        public bool TargetVisible;
        public float TargetContrast;
        public bool IsMoving;
        public bool WasMoving;
        public bool IsShooting;
        public int ShotCount;
        public float RecoilPitch;
        public float RecoilYaw;
        public float Spread;
        public float AccuracyRecovery;
        public bool IsPeeking;
        public bool EnemyAboutToPeek;
        public bool FlickDetected;
        public float FlickAngle;
        public float FlickSpeed;
        public string ActiveWeapon = "";
        public float WeaponRecoilPitch;
        public float WeaponRecoilYaw;
        public int WeaponMagazine;
        public int Health;
        public float ViewAnglePitch;
        public float ViewAngleYaw;

        public List<DetectedTarget> DetectedTargets = new();
        public DetectedTarget? BestTarget;
        public System.Numerics.Vector2 MouseDelta;
        public System.Numerics.Vector2 PreviousCrosshair;
        public bool AimKeyHeld;
    }
}
