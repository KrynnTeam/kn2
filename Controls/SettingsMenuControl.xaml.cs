using ShadowCheat.Class;
using ShadowCheat.UILibrary;
using System.Windows;
using System.Windows.Controls;

namespace ShadowCheat.Controls
{
    public partial class SettingsMenuControl : UserControl
    {
        private MainWindow? _mainWindow;
        private bool _isInitialized;

        public StackPanel ModelSettingsPanel => ModelSettings;
        public StackPanel SettingsConfigPanel => SettingsConfig;
        public StackPanel ThemeMenuPanel => ThemeMenu;
        public StackPanel DisplaySelectMenuPanel => DisplaySelectMenu;

        public SettingsMenuControl() { InitializeComponent(); }

        public void Initialize(MainWindow mainWindow)
        {
            if (_isInitialized) return;
            _mainWindow = mainWindow;
            _isInitialized = true;
        }
    }
}
