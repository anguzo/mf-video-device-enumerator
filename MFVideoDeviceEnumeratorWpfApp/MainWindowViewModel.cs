using System;
using System.Collections.ObjectModel;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.CIM;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.Common;

namespace MFVideoDeviceEnumeratorWpfApp
{
    public class MainWindowViewModel : IDisposable
    {
        private readonly VideoDeviceEnumerator _cimEnumerator;
        private readonly Enumerator.WinRT.VideoDeviceEnumerator _winRtEnumerator;
        private readonly Enumerator.WMI.VideoDeviceEnumerator _wmiEnumerator;

        public MainWindowViewModel()
        {
            _cimEnumerator = new VideoDeviceEnumerator();
            _wmiEnumerator = new Enumerator.WMI.VideoDeviceEnumerator();
            _winRtEnumerator = new Enumerator.WinRT.VideoDeviceEnumerator();
        }

        public ObservableCollection<IVideoDevice> CimDevices => _cimEnumerator.VideoDevices;
        public ObservableCollection<IVideoDevice> WmiDevices => _wmiEnumerator.VideoDevices;
        public ObservableCollection<IVideoDevice> WinRtDevices => _winRtEnumerator.VideoDevices;

        public void Dispose()
        {
            _cimEnumerator?.Dispose();
            _winRtEnumerator?.Dispose();
            _wmiEnumerator?.Dispose();
        }
    }
}