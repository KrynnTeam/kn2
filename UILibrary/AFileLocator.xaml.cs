using Microsoft.Win32;
using ShadowCheat.Class;

namespace ShadowCheat.UILibrary
{
    public partial class AFileLocator : System.Windows.Controls.UserControl
    {
        private string main_dictionary_path { get; set; }
        private string OFDFilter = "All files (*.*)|*.*";

        public AFileLocator(string title, string dictionary_path, string FileFilter = "All files (*.*)|*.*", string DLExtension = "")
        {
            InitializeComponent();
            DropdownTitle.Content = title;
            main_dictionary_path = dictionary_path;

            if (Dictionary.filelocationState.TryGetValue(main_dictionary_path, out var savedPath))
                FileLocationTextbox.Text = savedPath;

            OFDFilter = FileFilter;
        }

        private void OpenFileB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = System.IO.Directory.GetCurrentDirectory(),
                Filter = OFDFilter
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FileLocationTextbox.Text = openFileDialog.FileName;
                Dictionary.filelocationState[main_dictionary_path] = openFileDialog.FileName;
            }
        }
    }
}
