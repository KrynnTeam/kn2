using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ShadowCheat.Controls
{
    public partial class ModelMenuControl : UserControl
    {
        private MainWindow? _mainWindow;
        private bool _isInitialized;

        public ListBox ModelListBoxControl => ModelListBox;
        public ListBox ConfigsListBoxControl => ConfigsListBox;
        public Label SelectedModelNotifierControl => SelectedModelNotifier;
        public Label SelectedConfigNotifierControl => SelectedConfigNotifier;

        public ModelMenuControl() { InitializeComponent(); }

        public void Initialize(MainWindow mainWindow)
        {
            if (_isInitialized) return;
            _mainWindow = mainWindow;
            _isInitialized = true;
        }

        private void OpenFolderB_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "bin", btn.Tag.ToString()!);
                if (Directory.Exists(path))
                    Process.Start("explorer.exe", path);
            }
        }

        private void LocalModelSearchBox_TextChanged(object sender, TextChangedEventArgs e) =>
            FilterListBoxBySearch(ModelListBox, (TextBox)sender);

        private void LocalConfigSearchBox_TextChanged(object sender, TextChangedEventArgs e) =>
            FilterListBoxBySearch(ConfigsListBox, (TextBox)sender);

        private void FilterListBoxBySearch(ListBox listBox, TextBox searchBox)
        {
            string search = searchBox.Text?.ToLower() ?? "";
            foreach (var item in listBox.Items)
            {
                if (listBox.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem lbi)
                    lbi.Visibility = item.ToString()?.ToLower().Contains(search) == true
                        ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ModelListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                e.Effects = files.All(f => Path.GetExtension(f).Equals(".onnx", StringComparison.OrdinalIgnoreCase))
                    ? DragDropEffects.Copy : DragDropEffects.None;
            }
            else e.Effects = DragDropEffects.None;
        }

        private void ModelListBox_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "models");
            Directory.CreateDirectory(destDir);

            foreach (string file in files)
            {
                if (!Path.GetExtension(file).Equals(".onnx", StringComparison.OrdinalIgnoreCase)) continue;
                string fileName = Path.GetFileName(file);
                string destPath = Path.Combine(destDir, fileName);
                if (File.Exists(destPath))
                {
                    string dupPath = Path.Combine(destDir, Path.GetFileNameWithoutExtension(fileName) + "-DUPLICATED" + Path.GetExtension(fileName));
                    File.Move(file, dupPath);
                    continue;
                }
                try
                {
                    File.Move(file, destPath);
                    ModelListBox.Items.Add(new ListBoxItem { Content = Path.GetFileName(destPath), Tag = destPath });
                }
                catch { }
            }
        }

        private void ConfigsListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                e.Effects = files.All(f => Path.GetExtension(f).Equals(".cfg", StringComparison.OrdinalIgnoreCase))
                    ? DragDropEffects.Copy : DragDropEffects.None;
            }
            else e.Effects = DragDropEffects.None;
        }

        private void ConfigsListBox_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "configs");
            Directory.CreateDirectory(destDir);

            foreach (string file in files)
            {
                if (!Path.GetExtension(file).Equals(".cfg", StringComparison.OrdinalIgnoreCase)) continue;
                string fileName = Path.GetFileName(file);
                string destPath = Path.Combine(destDir, fileName);
                if (File.Exists(destPath))
                {
                    string dupPath = Path.Combine(destDir, Path.GetFileNameWithoutExtension(fileName) + "-DUPLICATED" + Path.GetExtension(fileName));
                    File.Move(file, dupPath);
                    continue;
                }
                try
                {
                    File.Move(file, destPath);
                    ConfigsListBox.Items.Add(new ListBoxItem { Content = Path.GetFileName(destPath), Tag = destPath });
                }
                catch { }
            }
        }
    }
}
