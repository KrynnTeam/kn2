namespace ShadowCheat.Class
{
    public static class Dictionary
    {
        public static Dictionary<string, bool> toggleState = new()
        {
            { "Aim Assist", false },
            { "Constant AI Tracking", false },
            { "Sticky Aim", false },
            { "Y Axis Percentage Adjustment", false },
            { "X Axis Percentage Adjustment", false },
            { "Predictions", false },
            { "EMA Smoothening", false },
            { "Auto Trigger", false },
            { "Cursor Check", false },
            { "Spray Mode", false },
            { "FOV", false },
            { "Dynamic FOV", false },
            { "Third Person Support", false },
            { "Show Detected Player", false },
            { "Show AI Confidence", false },
            { "Show Tracers", false },
            { "Collect Data While Playing", false },
            { "Auto Label Data", false },
            { "Mouse Background Effect", false },
            { "UI TopMost", false },
            { "Debug Mode", false },
            { "Enable Model Switch Keybind", false },
            { "StreamGuard", false }
        };

        public static Dictionary<string, double> sliderSettings = new()
        {
            { "Sticky Aim Threshold", 50 },
            { "Mouse Sensitivity (+/-)", 0.5 },
            { "Mouse Jitter", 0 },
            { "Y Offset (Up/Down)", 0 },
            { "Y Offset (%)", 50 },
            { "X Offset (Left/Right)", 0 },
            { "X Offset (%)", 50 },
            { "Kalman Lead Time", 0.1 },
            { "WiseTheFox Lead Time", 0.1 },
            { "Shalloe Lead Multiplier", 5 },
            { "EMA Smoothening", 0.5 },
            { "Auto Trigger Delay", 0.05 },
            { "FOV Size", 320 },
            { "Dynamic FOV Size", 320 },
            { "AI Confidence Font Size", 14 },
            { "Corner Radius", 10 },
            { "Border Thickness", 2 },
            { "Opacity", 0.8 },
            { "AI FPS Limit", 60 },
            { "AI Minimum Confidence", 50 },
            { "SelectedDisplay", 0 }
        };

        public static Dictionary<string, string> dropdownState = new()
        {
            { "Mouse Movement Method", "Mouse Event" },
            { "Movement Path", "Cubic Bezier" },
            { "Detection Area Type", "Closest to Center Screen" },
            { "Aiming Boundaries Alignment", "Center" },
            { "Tracer Position", "Bottom" },
            { "FOV Style", "Circle" },
            { "Prediction Method", "Kalman Filter" },
            { "Screen Capture Method", "DirectX" },
            { "Image Size", "640" },
            { "Target Class", "Best Confidence" },
            { "Model Type", "YOLOv8" }
        };

        public static Dictionary<string, string> bindingSettings = new()
        {
            { "Aim Keybind", "LButton" },
            { "Second Aim Keybind", "None" },
            { "Dynamic FOV Keybind", "None" },
            { "Model Switch Keybind", "None" },
            { "Emergency Stop Keybind", "F1" }
        };

        public static Dictionary<string, string> colorState = new()
        {
            { "FOV Color", "#FFFFFFFF" },
            { "Detected Player Color", "#FFFFFFFF" }
        };

        public static Dictionary<string, bool> minimizeState = new()
        {
            { "Aim Assist", false },
            { "Aim Config", false },
            { "Predictions", false },
            { "Auto Trigger", false },
            { "FOV Config", false },
            { "ESP Config", false },
            { "Model Settings", false },
            { "Settings Menu", false },
            { "Theme Settings", false },
            { "Screen Settings", false }
        };

        public static Dictionary<string, string> filelocationState = new()
        {
            { "ddxoft Driver Location", "" },
            { "Model Path", "" }
        };

        public static string lastLoadedModel = "N/A";
        public static string lastLoadedConfig = "N/A";
    }
}
