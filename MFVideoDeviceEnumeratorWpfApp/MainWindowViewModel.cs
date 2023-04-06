using System;
using System.Collections.ObjectModel;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.CIM;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.Common;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.WinRT;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.WMI;

namespace MFVideoDeviceEnumeratorWpfApp
{
    public class MainWindowViewModel : IDisposable
    {
        private readonly CimVideoDeviceManager _cimManager;
        private readonly WinRtVideoDeviceManager _winRtManager;
        private readonly WmiVideoDeviceManager _wmiManager;

        public MainWindowViewModel()
        {
            _cimManager = new CimVideoDeviceManager();
            _wmiManager = new WmiVideoDeviceManager();
            _winRtManager = new WinRtVideoDeviceManager();
        }

        public ObservableCollection<IVideoDevice> CimDevices => _cimManager.Devices;
        public ObservableCollection<IVideoDevice> WmiDevices => _wmiManager.Devices;
        public ObservableCollection<IVideoDevice> WinRtDevices => _winRtManager.Devices;

        public void Dispose()
        {
            _cimManager?.Dispose();
            _winRtManager?.Dispose();
            _wmiManager?.Dispose();
        }
    }
}