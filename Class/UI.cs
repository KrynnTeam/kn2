using ShadowCheat.UILibrary;

namespace ShadowCheat.Class
{
    public class UI
    {
        // Titles
        public ATitle? AT_Aim { get; set; }
        public ATitle? AT_AimConfig { get; set; }
        public ATitle? AT_Predictions { get; set; }
        public ATitle? AT_TriggerBot { get; set; }
        public ATitle? AT_FOV { get; set; }
        public ATitle? AT_DetectedPlayer { get; set; }
        public ATitle? AT_ModelSettings { get; set; }
        public ATitle? AT_SettingsMenu { get; set; }
        public ATitle? AT_ThemeColorWheel { get; set; }
        public ATitle? AT_DisplaySelector { get; set; }

        // Toggles - Aim Assist
        public AToggle? T_AimAligner { get; set; }
        public AToggle? T_ConstantAITracking { get; set; }
        public AToggle? T_StickyAim { get; set; }

        // Toggles - Predictions
        public AToggle? T_Predictions { get; set; }
        public AToggle? T_EMASmoothing { get; set; }

        // Toggles - Auto Trigger
        public AToggle? T_AutoTrigger { get; set; }
        public AToggle? T_CursorCheck { get; set; }
        public AToggle? T_SprayMode { get; set; }

        // Toggles - FOV
        public AToggle? T_FOV { get; set; }
        public AToggle? T_DynamicFOV { get; set; }
        public AToggle? T_ThirdPersonSupport { get; set; }

        // Toggles - ESP
        public AToggle? T_ShowDetectedPlayer { get; set; }
        public AToggle? T_ShowAIConfidence { get; set; }
        public AToggle? T_ShowTracers { get; set; }

        // Toggles - Settings
        public AToggle? T_CollectDataWhilePlaying { get; set; }
        public AToggle? T_AutoLabelData { get; set; }
        public AToggle? T_MouseBackgroundEffect { get; set; }
        public AToggle? T_UITopMost { get; set; }
        public AToggle? T_DebugMode { get; set; }
        public AToggle? T_StreamGuard { get; set; }

        // Toggles - Config
        public AToggle? T_YAxisPercentageAdjustment { get; set; }
        public AToggle? T_XAxisPercentageAdjustment { get; set; }
        public AToggle? T_EnableModelSwitchKeybind { get; set; }

        // KeyChangers
        public AKeyChanger? C_Keybind { get; set; }
        public AKeyChanger? C_SecondKeybind { get; set; }
        public AKeyChanger? C_DynamicFOV { get; set; }
        public AKeyChanger? C_ModelSwitchKeybind { get; set; }
        public AKeyChanger? C_EmergencyKeybind { get; set; }

        // Sliders - Aim Config
        public ASlider? S_MouseSensitivity { get; set; }
        public ASlider? S_MouseJitter { get; set; }
        public ASlider? S_StickyAimThreshold { get; set; }
        public ASlider? S_YOffset { get; set; }
        public ASlider? S_YOffsetPercent { get; set; }
        public ASlider? S_XOffset { get; set; }
        public ASlider? S_XOffsetPercent { get; set; }

        // Sliders - Predictions
        public ASlider? S_EMASmoothing { get; set; }
        public ASlider? S_KalmanLeadTime { get; set; }
        public ASlider? S_WiseTheFoxLeadTime { get; set; }
        public ASlider? S_ShalloeLeadMultiplier { get; set; }

        // Sliders - Trigger
        public ASlider? S_AutoTriggerDelay { get; set; }

        // Sliders - FOV
        public ASlider? S_FOVSize { get; set; }
        public ASlider? S_DynamicFOVSize { get; set; }

        // Sliders - ESP
        public ASlider? S_DPFontSize { get; set; }
        public ASlider? S_DPCornerRadius { get; set; }
        public ASlider? S_DPBorderThickness { get; set; }
        public ASlider? S_DPOpacity { get; set; }

        // Sliders - Model Settings
        public ASlider? S_AIFpsLimit { get; set; }
        public ASlider? S_AIMinimumConfidence { get; set; }

        // Dropdowns
        public ADropdown? D_MouseMovementMethod { get; set; }
        public ADropdown? D_MovementPath { get; set; }
        public ADropdown? D_DetectionAreaType { get; set; }
        public ADropdown? D_AimingBoundariesAlignment { get; set; }
        public ADropdown? D_PredictionMethod { get; set; }
        public ADropdown? D_FOVSTYLE { get; set; }
        public ADropdown? D_TracerPosition { get; set; }
        public ADropdown? D_ImageSize { get; set; }
        public ADropdown? D_TargetClass { get; set; }
        public ADropdown? D_ScreenCaptureMethod { get; set; }

        // Dropdown items
        public string? DDI_LGHUB { get; set; }
        public string? DDI_RazerSynapse { get; set; }
        public string? DDI_ddxoft { get; set; }
        public string? DDI_ClosestToCenterScreen { get; set; }

        // Buttons
        public APButton? B_SaveConfig { get; set; }
        public APButton? B_PerformanceHelper { get; set; }
        public APButton? B_RefreshDisplays { get; set; }

        // Special controls
        public AColorWheel? ThemeColorWheel { get; set; }
        public ADisplaySelector? DisplaySelector { get; set; }
        public AFileLocator? AFL_ddxoftDLLLocator { get; set; }
    }
}
