using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using Windows.Devices.Enumeration;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.Common;

namespace MFVideoDeviceEnumeratorWpfApp.Enumerator.WinRT
{
    public class VideoDeviceEnumerator : IDisposable
    {
        private readonly DeviceWatcher _deviceWatcher;

        public VideoDeviceEnumerator()
        {
            BindingOperations.EnableCollectionSynchronization(VideoDevices, new object());

            _deviceWatcher = DeviceInformation.CreateWatcher(DeviceClass.VideoCapture);

            _deviceWatcher.Added += UsbCameraAdded;
            _deviceWatcher.Removed += UsbCameraRemoved;
            _deviceWatcher.Updated += UsbCameraUpdated;

            _deviceWatcher.Start();
        }

        public ObservableCollection<IVideoDevice> VideoDevices { get; } = new ObservableCollection<IVideoDevice>();

        public void Dispose()
        {
            _deviceWatcher.Stop();
        }

        private async void UsbCameraAdded(DeviceWatcher sender, DeviceInformation args)
        {
            VideoDevices.Add(await WinRtVideoDevice.CreateInstanceAsync(args.Name, args.Id));
        }

        private void UsbCameraRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            try
            {
                VideoDevices.Remove(VideoDevices.First(vd => vd.SymbolicLink == args.Id));
            }
            catch (InvalidOperationException e)
            {
            }
        }

        private void UsbCameraUpdated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            Debug.WriteLine("USB camera updated: " + args.Id);
        }
    }
}