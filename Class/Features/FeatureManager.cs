namespace ShadowCheat.Class.Features
{
    public class FeatureManager : IDisposable
    {
        private readonly List<FeatureBase> _features = new();
        private readonly GameState _state = new();
        private Thread? _loopThread;
        private bool _running;
        private int _targetFps = 60;
        private readonly System.Diagnostics.Stopwatch _stopwatch = new();
        private TargetDetector _detector = new();
        private int _frameCount;
        private System.Numerics.Vector2 _lastMousePos;

        public IReadOnlyList<FeatureBase> Features => _features;
        public GameState State => _state;
        public bool IsRunning => _running;
        public TargetDetector Detector => _detector;

        public T AddFeature<T>() where T : FeatureBase, new()
        {
            var feature = new T();
            feature.Initialize();
            _features.Add(feature);
            return feature;
        }

        public T? GetFeature<T>() where T : FeatureBase =>
            _features.OfType<T>().FirstOrDefault();

        public bool IsEnabled<T>() where T : FeatureBase =>
            _features.OfType<T>().FirstOrDefault()?.Enabled ?? false;

        public void Toggle<T>() where T : FeatureBase
        {
            var feature = _features.OfType<T>().FirstOrDefault();
            if (feature != null) feature.Enabled = !feature.Enabled;
        }

        public void SetEnabled<T>(bool enabled) where T : FeatureBase
        {
            var feature = _features.OfType<T>().FirstOrDefault();
            if (feature != null) feature.Enabled = enabled;
        }

        public void Start()
        {
            if (_running) return;
            _running = true;
            _loopThread = new Thread(GameLoop) { IsBackground = true, Name = "FeatureManager" };
            _loopThread.Start();
        }

        public void Stop()
        {
            _running = false;
            _loopThread?.Join(1000);
            foreach (var f in _features) f.Shutdown();
        }

        private void GameLoop()
        {
            int targetDelay = 1000 / _targetFps;
            _stopwatch.Start();

            while (_running)
            {
                UpdateGameState();
                foreach (var feature in _features)
                {
                    if (feature.Enabled)
                        feature.Update(_state);
                }
                _frameCount++;

                int elapsed = (int)_stopwatch.ElapsedMilliseconds;
                int sleep = targetDelay - elapsed;
                if (sleep > 0) Thread.Sleep(sleep);
                _stopwatch.Restart();
            }
        }

        private void UpdateGameState()
        {
            float screenW = (float)System.Windows.SystemParameters.PrimaryScreenWidth;
            float screenH = (float)System.Windows.SystemParameters.PrimaryScreenHeight;
            float centerX = screenW / 2;
            float centerY = screenH / 2;

            _state.InGame = true;
            _state.CrosshairPosition = new System.Numerics.Vector2(centerX, centerY);

            var aimAssist = GetFeature<CrosshairPlacementAssist>();
            int aimKey = aimAssist?.AimKey ?? 0x02;
            bool requireKey = aimAssist?.RequireAimKey ?? true;
            _state.AimKeyHeld = !requireKey ||
                                InputSimulator.IsKeyDown(aimKey) ||
                                InputSimulator.IsKeyDown(0xA0) ||
                                InputSimulator.IsKeyDown(0xA1);

            // Mouse delta for flick detection
            var currentPos = System.Windows.Forms.Cursor.Position;
            _state.MouseDelta = new System.Numerics.Vector2(
                currentPos.X - _lastMousePos.X,
                currentPos.Y - _lastMousePos.Y);
            _lastMousePos = new System.Numerics.Vector2(currentPos.X, currentPos.Y);

            float flickSpeed = (float)Math.Sqrt(
                _state.MouseDelta.X * _state.MouseDelta.X +
                _state.MouseDelta.Y * _state.MouseDelta.Y);
            _state.FlickDetected = flickSpeed > 30;
            _state.FlickSpeed = flickSpeed;

            // Flick angle estimation
            if (_state.FlickDetected && Math.Abs(_state.MouseDelta.X) > 0)
            {
                float angleDeg = (float)(Math.Atan2(_state.MouseDelta.Y, _state.MouseDelta.X) * 180 / Math.PI);
                _state.FlickAngle = Math.Abs(angleDeg);
                if (_state.FlickAngle > 90) _state.FlickAngle = 180 - _state.FlickAngle;
            }

            // Run screen detection when needed
            bool needsDetection = _features.Any(f => f.Enabled && f.RequiresScreenDetection);
            if (_state.AimKeyHeld || needsDetection)
            {
                if (_detector.Profile.Mode == DetectionMode.Crosshair)
                {
                    var crosshairTarget = _detector.DetectCrosshair(centerX, centerY);
                    if (crosshairTarget != null)
                    {
                        _state.BestTarget = crosshairTarget;
                        _state.AimTarget = new System.Numerics.Vector2(centerX, centerY);
                        _state.TargetDistance = 0;
                        _state.TargetAngle = 0;
                        _state.TargetVisible = true;
                        _state.TargetContrast = 100f;
                    }
                    else
                    {
                        _state.BestTarget = null;
                        _state.TargetVisible = false;
                    }
                }
                else
                {
                    var targets = _detector.Detect(centerX, centerY);
                    _state.DetectedTargets = targets;

                    var best = targets
                        .Where(t => t.Confidence >= _detector.Profile.MinConfidence)
                        .OrderBy(t => t.DistanceFromCrosshair)
                        .FirstOrDefault();

                    if (best != null && best.Contrast >= _detector.Profile.MinContrast)
                    {
                        _state.BestTarget = best;
                        _state.AimTarget = new System.Numerics.Vector2(best.CenterX, best.CenterY);
                        _state.TargetDistance = best.DistanceFromCrosshair;
                        _state.TargetAngle = (float)(Math.Atan2(
                            best.CenterY - centerY, best.CenterX - centerX) * 180 / Math.PI);
                        _state.TargetVisible = true;
                        _state.TargetContrast = best.Contrast;
                    }
                    else
                    {
                        _state.BestTarget = null;
                        _state.TargetVisible = false;
                    }
                }
            }
            else
            {
                _state.DetectedTargets?.Clear();
                _state.BestTarget = null;
                _state.TargetVisible = false;
            }

            _state.IsShooting = InputSimulator.IsKeyDown(0x01);
            _state.WasMoving = _state.IsMoving;
            _state.IsMoving = InputSimulator.IsKeyDown(0x57) ||
                              InputSimulator.IsKeyDown(0x53) ||
                              InputSimulator.IsKeyDown(0x41) ||
                              InputSimulator.IsKeyDown(0x44);
        }

        public void Dispose() => Stop();
    }
}
