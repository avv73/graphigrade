using System.Runtime.InteropServices;

namespace GraphiGrade.Judge.Interop;

public static class WinAPI
{
    // P/Invoke declarations:
    [DllImport("user32.dll")]
    public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdc, uint nFlags);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);


    // RECT structure for GetWindowRect
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
