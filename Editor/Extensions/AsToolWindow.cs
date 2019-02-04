namespace Editor
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    /// Provides common extension methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Sets the window as a tool window.
        /// </summary>
        /// <param name="window">The window.</param>
        public static void AsToolWindow(this Window window)
        {
            if (window == null)
            {
                return;
            }

            window.ResizeMode = ResizeMode.NoResize;
            window.WindowStyle = WindowStyle.ToolWindow;

            var hwnd = new WindowInteropHelper(window).Handle;
            Win32.SetWindowLong(hwnd, Win32.GWL_STYLE, Win32.GetWindowLong(hwnd, Win32.GWL_STYLE) & ~Win32.WS_SYSMENU);
        }

        /// <summary>
        /// The Win32 members.
        /// </summary>
        private static class Win32
        {
#pragma warning disable SA1310 // Field names must not contain underscore // Justification = Win32 conventions
            internal const int GWL_STYLE = -16;
            internal const int WS_SYSMENU = 0x80000;
#pragma warning restore SA1310 // Field names must not contain underscore

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll")]
            internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        }
    }
}
