using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;

namespace EarTrumpet.UI.Helpers
{
    /// <summary>
    /// Icon for the second notification area entry -- the one that switches the
    /// default output rather than opening anything.
    /// </summary>
    public class DeviceCycleIconSource : IShellNotifyIconSource
    {
        public event Action<IShellNotifyIconSource> Changed;

        public Icon Current { get; private set; }

        private readonly DeviceCollectionViewModel _collection;
        private string _hash;

        public DeviceCycleIconSource(DeviceCollectionViewModel collection)
        {
            _collection = collection;
            _collection.TrayPropertyChanged += CheckForUpdate;

            CheckForUpdate();
        }

        // Nothing about this icon changes on hover.
        public void OnMouseOverChanged(bool isMouseOver) { }

        public void CheckForUpdate()
        {
            var nextHash = GetHash();
            if (nextHash == _hash)
            {
                return;
            }

            _hash = nextHash;

            using (var old = Current)
            {
                Current = Render();
                Changed?.Invoke(this);
            }
        }

        private Icon Render()
        {
            var size = User32.GetSystemMetricsForDpi(User32.SystemMetrics.SM_CXSMICON, WindowsTaskbar.Dpi);

            try
            {
                return TrayIconRenderer.RenderSwap(size, GetForegroundColor());
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"DeviceCycleIconSource Render Failed: {ex}");
                return null;
            }
        }

        private static Color GetForegroundColor()
        {
            if (SystemParameters.HighContrast)
            {
                var system = System.Windows.SystemColors.WindowTextColor;
                return Color.FromArgb(system.A, system.R, system.G, system.B);
            }

            // The taskbar follows the system theme, not the app theme, so this
            // tracks IsSystemLightTheme rather than the flyout's own setting.
            return SystemSettings.IsSystemLightTheme ? Color.Black : Color.White;
        }

        private string GetHash() =>
            $"dpi={WindowsTaskbar.Dpi} " +
            $"isSysLight={SystemSettings.IsSystemLightTheme} " +
            $"hc={SystemParameters.HighContrast}";
    }
}
