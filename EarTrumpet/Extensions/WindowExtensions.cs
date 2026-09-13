using EarTrumpet.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace EarTrumpet.Extensions
{
    public static class WindowExtensions
    {
        public static void SetWindowPos(this Window window, double top, double left, double height, double width)
        {
            User32.SetWindowPos(window.GetHandle(), IntPtr.Zero, (int)left, (int)top, (int)width, (int)height, User32.WindowPosFlags.SWP_NOZORDER | User32.WindowPosFlags.SWP_NOACTIVATE);
        }

        public static void RaiseWindow(this Window window)
        {
            window.Topmost = true;
            window.Activate();
            window.Topmost = false;
        }

        public static void Cloak(this Window window, bool hide = true)
        {
            int attributeValue = hide ? 1 : 0;
            DwmApi.DwmSetWindowAttribute(window.GetHandle(), DwmApi.DWMA_CLOAK, ref attributeValue, Marshal.SizeOf(attributeValue));
        }

        public static void EnableRoundedCornersIfApplicable(this Window window)
        {
            if (Environment.OSVersion.IsAtLeast(OSVersions.Windows11))
            {
                int attributeValue = (int)DwmApi.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                DwmApi.DwmSetWindowAttribute(window.GetHandle(), DwmApi.DWMWA_WINDOW_CORNER_PREFERENCE, ref attributeValue, Marshal.SizeOf(attributeValue));
            }
        }

        /// <summary>
        /// Asks the DWM to paint a system backdrop (Mica or Acrylic) behind the
        /// window, and makes the WPF render surface transparent so it shows.
        ///
        /// The DWM refuses both this and DWMWCP_ROUND on layered windows, so the
        /// caller must not set AllowsTransparency. That is also why
        /// EnableRoundedCornersIfApplicable silently did nothing on the flyout
        /// before this existed.
        /// </summary>
        /// <returns>True if the backdrop was applied.</returns>
        // Internal, not public: DwmApi and its nested enum are internal, and a
        // public method may not expose a less accessible parameter type.
        internal static bool TryEnableSystemBackdrop(this Window window, DwmApi.DWM_SYSTEMBACKDROP_TYPE backdrop)
        {
            if (!Environment.OSVersion.IsAtLeast(OSVersions.Windows11_22H2))
            {
                return false;
            }

            try
            {
                // Without this WPF clears the render surface to an opaque colour
                // and the backdrop never becomes visible.
                if (PresentationSource.FromVisual(window) is HwndSource source && source.CompositionTarget != null)
                {
                    source.CompositionTarget.BackgroundColor = Colors.Transparent;
                }

                // Negative margins extend the frame across the whole client area,
                // which is what gives the DWM something to draw the backdrop into.
                var margins = new DwmApi.MARGINS
                {
                    cxLeftWidth = -1,
                    cxRightWidth = -1,
                    cyTopHeight = -1,
                    cyBottomHeight = -1,
                };
                DwmApi.DwmExtendFrameIntoClientArea(window.GetHandle(), ref margins);

                int attributeValue = (int)backdrop;
                DwmApi.DwmSetWindowAttribute(window.GetHandle(), DwmApi.DWMWA_SYSTEMBACKDROP_TYPE, ref attributeValue, Marshal.SizeOf(attributeValue));
                return true;
            }
            catch (Exception ex)
            {
                // Insider builds and future releases have changed the accepted
                // attribute set before. Falling back to the acrylic path is a
                // cosmetic loss, not a reason to fail opening the flyout.
                Trace.WriteLine($"WindowExtensions TryEnableSystemBackdrop Failed: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Tells the DWM which theme the window is using, so the backdrop and
        /// any frame it draws are tinted to match.
        /// </summary>
        public static void SetImmersiveDarkMode(this Window window, bool isDarkMode)
        {
            if (!Environment.OSVersion.IsAtLeast(OSVersions.Windows11))
            {
                return;
            }

            try
            {
                int attributeValue = isDarkMode ? 1 : 0;
                DwmApi.DwmSetWindowAttribute(window.GetHandle(), DwmApi.DWMWA_USE_IMMERSIVE_DARK_MODE, ref attributeValue, Marshal.SizeOf(attributeValue));
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"WindowExtensions SetImmersiveDarkMode Failed: {ex}");
            }
        }

        public static void RemoveWindowStyle(this Window window, int styleToRemove)
        {
            var currentStyle = User32.GetWindowLong(window.GetHandle(), User32.GWL.GWL_STYLE);
            if (currentStyle == 0)
            {
                Trace.WriteLine($"WindowExtensions RemoveWindowStyle Failed: ({Marshal.GetLastWin32Error()})");
                return;
            }

            User32.SetWindowLong(window.GetHandle(), User32.GWL.GWL_STYLE, (currentStyle & ~styleToRemove));
        }

        public static void ApplyExtendedWindowStyle(this Window window, int newExStyle)
        {
            var currentExStyle = User32.GetWindowLong(window.GetHandle(), User32.GWL.GWL_EXSTYLE);
            if (currentExStyle == 0)
            {
                Trace.WriteLine($"WindowExtensions ApplyExtendedWindowStyle Failed: ({Marshal.GetLastWin32Error()})");
                return;
            }

            var oldExStyle = User32.SetWindowLong(window.GetHandle(), User32.GWL.GWL_EXSTYLE, currentExStyle | newExStyle);
            if (oldExStyle != currentExStyle)
            {
                Trace.WriteLine($"WindowExtensions ApplyExtendedWindowStyle Unexpected: ({oldExStyle} vs. {currentExStyle})");
                return;
            }
        }

        public static IntPtr GetHandle(this Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }
    }
}
