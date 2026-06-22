using System.Windows;

namespace ShadowCheat.UILibrary
{
    public partial class ATitle : System.Windows.Controls.UserControl
    {
        public ATitle(string text, bool minimizableMenu = false)
        {
            InitializeComponent();
            LabelTitle.Content = text;

            if (minimizableMenu)
                Minimize.Visibility = Visibility.Visible;
        }

        public void SetMinimizedIcon(bool minimized)
        {
            Minimize.Content = minimized ? "\xE710" : "\xE921";
        }
    }
}
