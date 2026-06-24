using System.Runtime.InteropServices;

namespace ShadowCheat.Class
{
    public static class InputSimulator
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            private readonly uint _unionPadding; // aligns union to 8 bytes on x64
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        private const uint INPUT_MOUSE = 0;

        public static void MoveMouse(int dx, int dy)
        {
            var input = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT { dx = dx, dy = dy, dwFlags = MOUSEEVENTF_MOVE, time = 0, dwExtraInfo = UIntPtr.Zero }
            };
            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }

        // Absolute + relative fallback — some games only respond to this
        public static void MoveMouseAbsolute(float targetX, float targetY)
        {
            int screenW = (int)System.Windows.SystemParameters.PrimaryScreenWidth;
            int screenH = (int)System.Windows.SystemParameters.PrimaryScreenHeight;
            int absX = (int)(targetX * 65535 / screenW);
            int absY = (int)(targetY * 65535 / screenH);

            var input = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT
                {
                    dx = absX,
                    dy = absY,
                    dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK,
                    time = 0,
                    dwExtraInfo = UIntPtr.Zero
                }
            };
            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }

        public static void MouseDown()
        {
            var input = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTDOWN, time = 0, dwExtraInfo = UIntPtr.Zero }
            };
            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }

        public static void MouseUp()
        {
            var input = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTUP, time = 0, dwExtraInfo = UIntPtr.Zero }
            };
            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }

        public static void Click()
        {
            MouseDown();
            Thread.Sleep(1);
            MouseUp();
        }

        public static bool IsKeyDown(int vKey) => (GetAsyncKeyState(vKey) & 0x8000) != 0;

        public static void MoveMouseRelative(float sensitivity, float targetX, float targetY, float cursorX, float cursorY)
        {
            float deltaX = targetX - cursorX;
            float deltaY = targetY - cursorY;
            float moveX = deltaX * sensitivity * 0.35f;
            float moveY = deltaY * sensitivity * 0.35f;
            moveX = Math.Clamp(moveX, -30, 30);
            moveY = Math.Clamp(moveY, -30, 30);
            if (Math.Abs(moveX) > 0.5f || Math.Abs(moveY) > 0.5f)
                MoveMouse((int)moveX, (int)moveY);
        }
    }
}
