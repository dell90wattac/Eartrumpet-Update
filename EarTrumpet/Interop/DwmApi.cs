using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    class DwmApi
    {
        internal const int DWMA_CLOAK = 13;
        internal const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        internal const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        internal const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

        internal enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        internal enum DWM_SYSTEMBACKDROP_TYPE
        {
            DWMSBT_AUTO = 0,
            DWMSBT_NONE = 1,
            // Mica. Intended for long-lived main windows.
            DWMSBT_MAINWINDOW = 2,
            // Acrylic. What Windows 11 uses for flyouts and context menus.
            DWMSBT_TRANSIENTWINDOW = 3,
            // Mica Alt. Tabbed-window variant, more strongly wallpaper-tinted.
            DWMSBT_TABBEDWINDOW = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize);

        [DllImport("dwmapi.dll")]
        internal static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref MARGINS margins);
    }
}
