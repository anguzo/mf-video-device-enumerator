using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace MFVideoDeviceEnumeratorWpfApp.Enumerator.Common
{
    public abstract class VideoDeviceManager
    {
        private readonly ConcurrentDictionary<string, IVideoDevice> _deviceMap =
            new ConcurrentDictionary<string, IVideoDevice>();

        private readonly Dispatcher _dispatcher;

        protected VideoDeviceManager()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public ObservableCollection<IVideoDevice> Devices { get; } = new ObservableCollection<IVideoDevice>();

        public void Add(IVideoDevice device)
        {
            if (_deviceMap.TryAdd(device.SymbolicLink, device))
                _dispatcher.Invoke(() => { Devices.Add(device); });
        }

        public void Remove(string deviceId)
        {
            if (_deviceMap.TryRemove(deviceId, out var device))
                _dispatcher.Invoke(() => { Devices.Remove(device); });
        }

        public bool TryFind(string deviceId, out IVideoDevice found)
        {
            if (deviceId == null)
            {
                found = null;
                return false;
            }

            return _deviceMap.TryGetValue(deviceId, out found);
        }
    }
}