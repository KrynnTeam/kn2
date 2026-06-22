namespace ShadowCheat.UILibrary
{
    public partial class APButton : System.Windows.Controls.UserControl
    {
        public APButton(string Text, string? tooltip = null, string iconGlyph = "\uE8B0")
        {
            InitializeComponent();
            ButtonTitle.Content = Text;
            IconLabel.Content = iconGlyph;
            if (!string.IsNullOrEmpty(tooltip))
                ToolTip = new System.Windows.Controls.ToolTip { Content = tooltip };
        }
    }
}
