using System.Management;
using System.Windows;
using System.Windows.Controls;

namespace ShadowCheat.Controls
{
    public partial class AboutPage : UserControl
    {
        private MainWindow? _mainWindow;

        public AboutPage() { InitializeComponent(); }

        public void Initialize(MainWindow mainWindow)
        {
            if (_mainWindow != null) return;
            _mainWindow = mainWindow;
            LoadSystemSpecs();
        }

        private void LoadSystemSpecs()
        {
            try
            {
                string cpu = "", gpu = "", ram = "";
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                    foreach (var obj in searcher.Get()) cpu = obj["Name"]?.ToString() ?? "Unknown";
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                    foreach (var obj in searcher.Get()) { gpu = obj["Name"]?.ToString() ?? "Unknown"; break; }
                using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                    foreach (var obj in searcher.Get())
                    {
                        long bytes = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                        ram = $"{bytes / (1024 * 1024 * 1024.0):F1} GB";
                        break;
                    }
                AboutSpecs.Text = $"CPU: {cpu}\nGPU: {gpu}\nRAM: {ram}";
            }
            catch { AboutSpecs.Text = "System info unavailable"; }
        }
    }
}