using System;
using System.Runtime.InteropServices;

namespace Frost.Helpers
{
    public static class WindowMessageHook
    {
        // Define the delegate with appropriate types for WndProc
        private delegate IntPtr WndProcDelegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, WndProcDelegate newProc);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int GWLP_WNDPROC = -4;
        private static WndProcDelegate? newWndProcDelegate;
        private static IntPtr prevWndProc = IntPtr.Zero;
        private static Func<IntPtr, int, IntPtr, IntPtr, IntPtr>? messageHandler;

        // Attach method to hook into the window message loop
        public static void Attach(IntPtr hwnd, Func<IntPtr, int, IntPtr, IntPtr, IntPtr> handler)
        {
            messageHandler = handler;
            newWndProcDelegate = WndProc;
            prevWndProc = SetWindowLongPtr(hwnd, GWLP_WNDPROC, newWndProcDelegate);
        }

        // WndProc method that receives and handles the window messages
        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            // Call the message handler provided by the caller
            return messageHandler?.Invoke(hwnd, msg, wParam, lParam) ?? IntPtr.Zero;
        }
    }
}
