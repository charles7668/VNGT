using System.Runtime.InteropServices;

namespace GameManager
{
    public static class WindowsAPI
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}