using ShadowCheat.Other;

namespace ShadowCheat.UILibrary
{
    public partial class AKeyChanger : System.Windows.Controls.UserControl
    {
        public AKeyChanger(string Text, string Keybind, string? tooltip = null)
        {
            InitializeComponent();
            KeyChangerTitle.Content = Text;
            if (!string.IsNullOrEmpty(tooltip))
                ToolTip = new System.Windows.Controls.ToolTip { Content = tooltip };
            KeyNotifier.Content = KeybindNameManager.ConvertToRegularKey(Keybind);
        }
    }
}
