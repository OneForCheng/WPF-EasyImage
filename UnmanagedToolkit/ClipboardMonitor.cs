using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace UnmanagedToolkit
{
    public sealed class ClipboardMonitor : IDisposable
    {
        private static class NativeMethods
        {
            /// <summary>
            /// Places the given window in the system-maintained clipboard format listener list.
            /// </summary>
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AddClipboardFormatListener(IntPtr hwnd);

            /// <summary>
            /// Removes the given window from the system-maintained clipboard format listener list.
            /// </summary>
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

            /// <summary>
            /// Sent when the contents of the clipboard have changed.
            /// </summary>
            public const int WmClipboardupdate = 0x031D;

            /// <summary>
            /// To find message-only windows, specify HWND_MESSAGE in the hwndParent parameter of the FindWindowEx function.
            /// </summary>
            public static readonly IntPtr HwndMessage = new IntPtr(-3);
        }

        private readonly HwndSource _hwndSource = new HwndSource(0, 0, 0, 0, 0, 0, 0, null, NativeMethods.HwndMessage);

        public ClipboardMonitor()
        {
            _hwndSource.AddHook(WndProc);
            NativeMethods.AddClipboardFormatListener(_hwndSource.Handle);
        }

        public void Dispose()
        {
            NativeMethods.RemoveClipboardFormatListener(_hwndSource.Handle);
            _hwndSource.RemoveHook(WndProc);
            _hwndSource.Dispose();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WmClipboardupdate)
            {
                OnClipboardContentChanged?.Invoke(this, EventArgs.Empty);
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Occurs when the clipboard content changes.
        /// </summary>
        public event EventHandler OnClipboardContentChanged;
    }

}
