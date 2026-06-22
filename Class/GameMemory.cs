using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ShadowCheat.Class
{
    public static class GameMemory
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint PROCESS_VM_READ = 0x0010;
        private const uint PROCESS_VM_WRITE = 0x0020;
        private const uint PROCESS_VM_OPERATION = 0x0008;

        private static IntPtr _processHandle;
        private static int _processId;
        private static IntPtr _moduleBase;

        public static bool Attach(string processName)
        {
            var procs = Process.GetProcessesByName(processName);
            if (procs.Length == 0) return false;
            var proc = procs[0];
            _processId = proc.Id;
            _processHandle = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, false, _processId);
            if (_processHandle == IntPtr.Zero) return false;
            _moduleBase = proc.MainModule!.BaseAddress;
            return true;
        }

        public static void Detach()
        {
            if (_processHandle != IntPtr.Zero)
                CloseHandle(_processHandle);
        }

        public static T Read<T>(IntPtr address) where T : unmanaged
        {
            byte[] buffer = new byte[Marshal.SizeOf<T>()];
            ReadProcessMemory(_processHandle, address, buffer, buffer.Length, out _);
            return MemoryMarshal.Read<T>(buffer);
        }

        public static void Write<T>(IntPtr address, T value) where T : unmanaged
        {
            byte[] buffer = new byte[Marshal.SizeOf<T>()];
            MemoryMarshal.Write(buffer, ref value);
            WriteProcessMemory(_processHandle, address, buffer, buffer.Length, out _);
        }

        public static IntPtr GetAddress(params int[] offsets)
        {
            IntPtr addr = _moduleBase;
            for (int i = 0; i < offsets.Length; i++)
            {
                if (i < offsets.Length - 1)
                    addr = Read<IntPtr>(addr + offsets[i]);
                else
                    addr += offsets[i];
            }
            return addr;
        }

        public static bool IsAttached => _processHandle != IntPtr.Zero;
        public static int ProcessId => _processId;
    }
}
