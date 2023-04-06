using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Devices.Enumeration;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.Common;

namespace MFVideoDeviceEnumeratorWpfApp.Enumerator.WinRT
{
    public class WinRtVideoDeviceManager : VideoDeviceManager, IDisposable
    {
        private readonly DeviceWatcher _deviceWatcher;

        public WinRtVideoDeviceManager()
        {
            _deviceWatcher = DeviceInformation.CreateWatcher(DeviceClass.VideoCapture);

            _deviceWatcher.Added += UsbCameraAdded;
            _deviceWatcher.Removed += UsbCameraRemoved;
            _deviceWatcher.Updated += UsbCameraUpdated;

            _deviceWatcher.Start();
        }


        public void Dispose()
        {
            _deviceWatcher.Stop();
        }

        private async void UsbCameraAdded(DeviceWatcher sender, DeviceInformation args)
        {
            try
            {
                var device = await WinRtVideoDevice.CreateInstanceAsync(args.Name, args.Id);
                Add(device);
            }
            catch (COMException e)
            {
                Debug.WriteLine(e);
            }
            catch (DirectoryNotFoundException e)
            {
                Debug.WriteLine(e);
            }
        }

        private void UsbCameraRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            try
            {
                Remove(args.Id);
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine(e);
            }
        }

        private void UsbCameraUpdated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if (TryFind(args.Id, out var device)) return;
            // device.DevicePropertiesChanged();
        }
    }
}