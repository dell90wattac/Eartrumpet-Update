using EarTrumpet.DataModel;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Drawing;

namespace EarTrumpet.UI.Helpers
{
    public class TaskbarIconSource : IShellNotifyIconSource
    {
        enum IconKind
        {
            EarTrumpet,
            EarTrumpet_LightTheme,
            Muted,
            SpeakerZeroBars,
            SpeakerOneBar,
            SpeakerTwoBars,
            SpeakerThreeBars,
            NoDevice,
        }

        public event Action<IShellNotifyIconSource> Changed;

        public Icon Current { get; private set; }

        private readonly DeviceCollectionViewModel _collection;
        private readonly AppSettings _settings;
        private bool _isMouseOver;
        private string _hash;
        private IconKind _kind;

        public TaskbarIconSource(DeviceCollectionViewModel collection, AppSettings settings)
        {
            _collection = collection;
            _settings = settings;

            _settings.UseLegacyIconChanged += (_, __) => CheckForUpdate();
            collection.TrayPropertyChanged += OnTrayPropertyChanged;

            OnTrayPropertyChanged();
        }

        public void OnMouseOverChanged(bool isMouseOver)
        {
            _isMouseOver = isMouseOver;
            CheckForUpdate();
        }

        public void CheckForUpdate()
        {
            var nextHash = GetHash();
            if (nextHash != _hash)
            {
                Trace.WriteLine($"TaskbarIconSource Changed: {nextHash}");
                _hash = nextHash;
                using (var old = Current)
                {
                    Current = SelectAndLoadIcon(_kind);
                    Changed?.Invoke(this);
                }
            }
        }

        private void OnTrayPropertyChanged()
        {
            _kind = IconKindFromDeviceCollection(_collection);
            CheckForUpdate();
        }

        private Icon SelectAndLoadIcon(IconKind kind)
        {
            if (_settings.UseLegacyIcon)
            {
                return LoadIcon(SystemSettings.IsSystemLightTheme ? IconKind.EarTrumpet_LightTheme : IconKind.EarTrumpet);
            }

            try
            {
                var size = User32.GetSystemMetricsForDpi(User32.SystemMetrics.SM_CXSMICON, WindowsTaskbar.Dpi);
                var device = _collection.Default;

                return TrayIconRenderer.RenderLevel(
                    size,
                    GetForegroundColor(),
                    device?.Volume ?? 0,
                    device != null && device.IsMuted,
                    device != null);
            }
            catch (Exception ex)
            {
                // Never leave the notification area without an icon.
                Trace.WriteLine($"TaskbarIconSource RenderLevel Failed: {ex}");
                return LoadIcon(SystemSettings.IsSystemLightTheme ? IconKind.EarTrumpet_LightTheme : IconKind.EarTrumpet);
            }
        }

        private Color GetForegroundColor()
        {
            if (System.Windows.SystemParameters.HighContrast)
            {
                var system = _isMouseOver
                    ? System.Windows.SystemColors.HighlightTextColor
                    : System.Windows.SystemColors.WindowTextColor;
                return Color.FromArgb(system.A, system.R, system.G, system.B);
            }

            // The taskbar follows the system theme rather than the app theme.
            return SystemSettings.IsSystemLightTheme ? Color.Black : Color.White;
        }

        private static Icon LoadIcon(IconKind kind)
        {
            uint dpi = WindowsTaskbar.Dpi;
            switch (kind)
            {
                case IconKind.EarTrumpet:
                    return IconHelper.LoadIconForTaskbar((string)App.Current.Resources["EarTrumpetIconDark"], dpi);
                case IconKind.EarTrumpet_LightTheme:
                    return IconHelper.LoadIconForTaskbar((string)App.Current.Resources["EarTrumpetIconLight"], dpi);
                case IconKind.Muted:
                    return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.Muted), dpi);
                case IconKind.NoDevice:
                    return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.NoDevice), dpi);
                case IconKind.SpeakerZeroBars:
                    return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.SpeakerZeroBars), dpi);
                case IconKind.SpeakerOneBar:
                    return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.SpeakerOneBar), dpi);
                case IconKind.SpeakerTwoBars:
                    return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.SpeakerTwoBars), dpi);
                case IconKind.SpeakerThreeBars:
                    return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.SpeakerThreeBars), dpi);
                default: throw new NotImplementedException();
            }
        }

        private string GetHash() =>
            $"kind={_kind} " +
            $"level={GetLevelBucket()} " +
            $"{(System.Windows.SystemParameters.HighContrast ? $"hc=true mouse={_isMouseOver} " : "")}" +
            $"dpi={WindowsTaskbar.Dpi} " +
            $"isSysLight={SystemSettings.IsSystemLightTheme} " +
            $"isLegacy={_settings.UseLegacyIcon}";

        // Only the lit-bar count can change the drawing, so only it belongs in
        // the hash.
        private int GetLevelBucket()
        {
            var device = _collection.Default;
            if (device == null || device.IsMuted)
            {
                return -1;
            }

            return device.Volume == 0 ? 0 : (int)Math.Ceiling(device.Volume / 25f);
        }

        private static IconKind IconKindFromDeviceCollection(DeviceCollectionViewModel collectionViewModel)
        {
            if (collectionViewModel.Default != null)
            {
                switch (collectionViewModel.Default.IconKind)
                {
                    case DeviceViewModel.DeviceIconKind.Mute:
                        return IconKind.Muted;
                    case DeviceViewModel.DeviceIconKind.Bar0:
                        return IconKind.SpeakerZeroBars;
                    case DeviceViewModel.DeviceIconKind.Bar1:
                        return IconKind.SpeakerOneBar;
                    case DeviceViewModel.DeviceIconKind.Bar2:
                        return IconKind.SpeakerTwoBars;
                    case DeviceViewModel.DeviceIconKind.Bar3:
                        return IconKind.SpeakerThreeBars;
                    default: throw new NotImplementedException();
                }
            }
            return IconKind.NoDevice;
        }
    }
}
