using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;

namespace ShadowCheat.Class.Features
{
    public class HwidSpoofing : FeatureBase
    {
        public override string Name => "HWID Spoofing";

        private bool _isSpoofed;
        private readonly Dictionary<string, string> _originals = new();
        private static readonly Random _rng = new();

        public override void Update(GameState state)
        {
            if (Enabled && !_isSpoofed)
            {
                if (!IsAdministrator())
                {
                    RequestElevationOrExit();
                    return;
                }
                BackupAndSpoof();
                _isSpoofed = true;
            }
            else if (!Enabled && _isSpoofed)
            {
                Restore();
                _isSpoofed = false;
            }
        }

        public static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RequestElevationOrExit()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show(
                    "HWID Spoofing requiere permisos de Administrador para modificar " +
                    "el registro del sistema (HKLM).\n\n" +
                    "¿Reiniciar la aplicación como Administrador?",
                    "Se requieren permisos de Administrador",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var exePath = Environment.ProcessPath;
                        if (exePath != null)
                        {
                            Process.Start(new ProcessStartInfo(exePath)
                            {
                                UseShellExecute = true,
                                Verb = "runas"
                            });
                        }
                    }
                    catch
                    {
                        MessageBox.Show(
                            "No se pudo reiniciar como Administrador. " +
                            "La aplicación se cerrará.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }

                Application.Current.Shutdown();
            });
        }

        private void BackupAndSpoof()
        {
            SpoofReg("SOFTWARE\\Microsoft\\Cryptography", "MachineGuid", Guid.NewGuid().ToString("D").ToUpper());
            SpoofReg("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductId", RandProductId());
            SpoofReg("HARDWARE\\DESCRIPTION\\System\\BIOS", "SystemSerialNumber", RandSerial());
            SpoofReg("HARDWARE\\DESCRIPTION\\System\\BIOS", "BaseBoardSerialNumber", RandSerial());
            SpoofMac();
            SpoofVolumeSerial();
            CleanTraces();
        }

        private void SpoofReg(string path, string valueName, string newValue)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(path, writable: true);
                if (key == null) return;
                var existing = key.GetValue(valueName)?.ToString();
                if (existing != null)
                {
                    var dictKey = "HKLM\\" + path + "\\" + valueName;
                    if (!_originals.ContainsKey(dictKey))
                        _originals[dictKey] = existing;
                }
                key.SetValue(valueName, newValue);
            }
            catch { }
        }

        private void Restore()
        {
            foreach (var kvp in _originals)
            {
                try
                {
                    var sep = kvp.Key.IndexOf('\\', 5);
                    if (sep < 0) continue;
                    var path = kvp.Key.Substring(5, sep - 5);
                    var name = kvp.Key.Substring(sep + 1);
                    using var key = Registry.LocalMachine.OpenSubKey(path, writable: true);
                    if (key == null) continue;
                    var current = key.GetValue(name)?.ToString();
                    if (current != null)
                        key.SetValue(name, kvp.Value);
                }
                catch { }
            }
            _originals.Clear();
        }

        private void SpoofMac()
        {
            var basePath = "SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}";
            try
            {
                using var root = Registry.LocalMachine.OpenSubKey(basePath);
                if (root == null) return;
                foreach (var sub in root.GetSubKeyNames())
                {
                    if (!int.TryParse(sub, out _)) continue;
                    using var key = Registry.LocalMachine.OpenSubKey(basePath + "\\" + sub, writable: true);
                    if (key == null) continue;
                    var cid = key.GetValue("ComponentId")?.ToString() ?? "";
                    if (cid.Contains("VM") || cid.Contains("Virtual") || cid.Contains("Loopback")) continue;
                    if (key.GetValue("DriverDate") == null) continue;
                    var orig = key.GetValue("NetworkAddress")?.ToString();
                    if (!string.IsNullOrEmpty(orig))
                        _originals["MAC_" + sub] = orig;
                    key.SetValue("NetworkAddress", RandMac());
                }
            }
            catch { }
        }

        private static void SpoofVolumeSerial()
        {
            try
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.DriveType == DriveType.Fixed && drive.IsReady)
                    {
                        var label = "VOL_" + _rng.Next(100000, 999999);
                        SetVolumeLabel(drive.Name.TrimEnd('\\'), label);
                    }
                }
            }
            catch { }
        }

        private static void CleanTraces()
        {
            var targets = new[]
            {
                "%ProgramData%\\Riot Games",
                "%ProgramData%\\EasyAntiCheat",
                "%ProgramData%\\BattlEye",
                "%temp%",
                "%SystemRoot%\\Prefetch"
            };
            foreach (var raw in targets)
            {
                try
                {
                    var dir = Environment.ExpandEnvironmentVariables(raw);
                    if (!Directory.Exists(dir)) continue;
                    foreach (var f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                    {
                        try { File.Delete(f); } catch { }
                    }
                }
                catch { }
            }
        }

        private static string RandProductId()
        {
            return $"{_rng.Next(10000, 99999)}-{_rng.Next(100, 999)}-{_rng.Next(1000, 9999)}-{_rng.Next(100, 999)}{_rng.Next(1000, 9999)}";
        }

        private static string RandSerial()
        {
            var c = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var r = new char[12];
            for (int i = 0; i < 12; i++) r[i] = c[_rng.Next(c.Length)];
            return new string(r);
        }

        private static string RandMac()
        {
            var b = new byte[6];
            _rng.NextBytes(b);
            b[0] = (byte)(b[0] & 0xFE | 0x02);
            return BitConverter.ToString(b).Replace('-', ':');
        }

        public string GetVerificationText()
        {
            var sb = new System.Text.StringBuilder();

            void AppendReg(string label, string path, string name)
            {
                try
                {
                    using var k = Registry.LocalMachine.OpenSubKey(path);
                    sb.AppendLine($"{label}: {k?.GetValue(name) ?? "(no encontrado)"}");
                }
                catch { sb.AppendLine($"{label}: (error de lectura)"); }
            }

            sb.AppendLine("═══ Valores Actuales ═══");
            AppendReg("MachineGuid", @"SOFTWARE\Microsoft\Cryptography", "MachineGuid");
            AppendReg("ProductId", @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductId");
            AppendReg("SystemSerial", @"HARDWARE\DESCRIPTION\System\BIOS", "SystemSerialNumber");
            AppendReg("BaseBoardSerial", @"HARDWARE\DESCRIPTION\System\BIOS", "BaseBoardSerialNumber");

            sb.AppendLine("\n--- MAC Addresses ---");
            try
            {
                var bp = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}";
                using var root = Registry.LocalMachine.OpenSubKey(bp);
                if (root != null)
                {
                    foreach (var sub in root.GetSubKeyNames())
                    {
                        if (!int.TryParse(sub, out _)) continue;
                        using var k = Registry.LocalMachine.OpenSubKey(bp + "\\" + sub);
                        if (k == null) continue;
                        var desc = k.GetValue("DriverDesc")?.ToString() ?? sub;
                        var mac = k.GetValue("NetworkAddress")?.ToString();
                        if (mac != null) sb.AppendLine($"  {desc}: {mac}");
                    }
                }
            }
            catch { sb.AppendLine("  (error de lectura)"); }

            if (_isSpoofed && _originals.Count > 0)
            {
                sb.AppendLine("\n═══ Valores Originales (respaldados) ═══");
                foreach (var kvp in _originals)
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
            else
            {
                sb.AppendLine("\n(No hay spoofing activo — los valores mostrados son los originales)");
            }

            return sb.ToString();
        }

        public override void Initialize() { }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetVolumeLabel(string rootPathName, string volumeName);
    }
}
