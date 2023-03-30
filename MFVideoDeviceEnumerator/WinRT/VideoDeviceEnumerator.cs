using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.Devices.Enumeration;
using Vortice.MediaFoundation;

namespace MFVideoDeviceEnumerator.WinRT
{
    public class VideoDeviceEnumerator : IDisposable
    {
        private readonly DeviceWatcher _deviceWatcher;
        private readonly SynchronizationContext _synchronizationContext;
        public VideoDeviceEnumerator()
        {
            _deviceWatcher = DeviceInformation.CreateWatcher(DeviceClass.VideoCapture);
            _synchronizationContext = SynchronizationContext.Current;

            _deviceWatcher.Added += UsbCameraAdded;
            _deviceWatcher.Removed += UsbCameraRemoved;
            _deviceWatcher.Updated += UsbCameraUpdated;

            _deviceWatcher.Start();
        }

        public ObservableCollection<VideoDevice> VideoDevices { get; } = new ObservableCollection<VideoDevice>();

        public void Dispose()
        {
            _deviceWatcher.Stop();
        }

        private void UsbCameraAdded(DeviceWatcher sender, DeviceInformation args)
        {
            _synchronizationContext.Post(async (state) =>
            {
                VideoDevices.Add(await VideoDevice.CreateInstance(args.Name, args.Id, CaptureDeviceAttributeKeys.SourceTypeVidcap));
            }, null);
            Debug.WriteLine("USB camera added: " + args.Id);
        }

        private void UsbCameraRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            _synchronizationContext.Post((state) =>
            {
                VideoDevices.Remove(VideoDevices.First(vd => vd.SymbolicLink == args.Id));
            }, null);
            Debug.WriteLine("USB camera removed: " + args.Id);
        }

        private void UsbCameraUpdated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            Debug.WriteLine("USB camera updated: " + args.Id);
        }
    }
}