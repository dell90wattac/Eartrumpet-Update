using EarTrumpet.UI.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    /// <summary>
    /// One device row in the flyout: the current default endpoint for a data
    /// flow, the list of endpoints to switch to, and whether that list is open.
    ///
    /// Identical for playback and recording, because the underlying
    /// DeviceCollectionViewModel is already parameterised on
    /// IAudioDeviceManager -- switching a microphone is the same operation as
    /// switching headphones, so it should be the same gesture.
    /// </summary>
    public class DevicePickerViewModel : BindableBase
    {
        /// <summary>Short label above the device name -- "Output" or "Input".</summary>
        public string KindLabel { get; }

        /// <summary>
        /// Every endpoint that can be picked. The manager enumerates with
        /// DeviceState.ACTIVE and drops devices as they become unplugged or
        /// disabled, so this is already the list of usable endpoints.
        /// </summary>
        public ObservableCollection<DeviceViewModel> Devices => _collection.AllDevices;

        public DeviceViewModel Default => _collection.Default;
        public bool HasDevice => _collection.Default != null;

        /// <summary>Nothing to choose between with one endpoint.</summary>
        public bool CanPick => _collection.AllDevices.Count > 1;

        /// <summary>
        /// Whether this row carries a level control at all. The input row does
        /// not: switching microphone is the useful action, while its gain is
        /// better left to Windows or to whichever app is listening.
        /// </summary>
        public bool ShowsVolume { get; }

        /// <summary>
        /// Whether releasing this row's slider sounds the system feedback tone
        /// -- the "dong" that tells you how loud you just set things. True for
        /// playback only: the tone comes out of the speakers, so playing it
        /// while dragging a microphone level tells you nothing.
        /// </summary>
        public bool PlaysVolumeFeedback { get; }

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen != value)
                {
                    _isOpen = value;
                    RaisePropertyChanged(nameof(IsOpen));
                }
            }
        }

        public ICommand Toggle { get; }
        public ICommand Select { get; }

        private readonly DeviceCollectionViewModel _collection;
        private bool _isOpen;

        public DevicePickerViewModel(DeviceCollectionViewModel collection, string kindLabel, bool showsVolume, bool playsVolumeFeedback)
        {
            _collection = collection;
            KindLabel = kindLabel;
            ShowsVolume = showsVolume;
            PlaysVolumeFeedback = playsVolumeFeedback;

            _collection.DefaultChanged += (_, __) =>
            {
                RaisePropertyChanged(nameof(Default));
                RaisePropertyChanged(nameof(HasDevice));
            };
            _collection.AllDevices.CollectionChanged += OnDevicesChanged;

            Toggle = new RelayCommand(() => IsOpen = !IsOpen);
            Select = new RelayCommand<DeviceViewModel>(device =>
            {
                device?.MakeDefaultDevice();
                IsOpen = false;
            });
        }

        private void OnDevicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CanPick));

            // The endpoint being chosen from may have just gone away.
            if (!CanPick)
            {
                IsOpen = false;
            }
        }

        public void Close() => IsOpen = false;
    }
}
