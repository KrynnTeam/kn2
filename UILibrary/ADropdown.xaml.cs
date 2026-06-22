using System.Windows.Controls;
using ShadowCheat.Class;

namespace ShadowCheat.UILibrary
{
    public partial class ADropdown : System.Windows.Controls.UserControl
    {
        private string main_dictionary_path { get; set; }

        public ADropdown(string title, string dictionary_path, string? tooltip = null)
        {
            InitializeComponent();
            DropdownTitle.Content = title;
            main_dictionary_path = dictionary_path;
            if (!string.IsNullOrEmpty(tooltip))
                ToolTip = new System.Windows.Controls.ToolTip { Content = tooltip };
        }

        private void DropdownBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DropdownBox.SelectedItem is string s)
            {
                Dictionary.dropdownState[main_dictionary_path] = s;
            }
        }
    }
}
