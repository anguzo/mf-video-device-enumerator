using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Windows.Data;

using Vortice.MediaFoundation;

namespace MFVideoDeviceEnumerator
{
    public class VideoDeviceEnumerator : IDisposable
    {
        public static readonly Guid KSCATEGORY_VIDEO_CAMERA =
            new Guid(0xe5323777, 0xf976, 0x4f5b, 0x9b, 0x55, 0xb9, 0x46, 0x99, 0xc4, 0x6e, 0x44);


        private readonly ManagementEventWatcher _creationWatcher;
        private readonly ManagementEventWatcher _deletionWatcher;

        private readonly object _lock = new object();

        public VideoDeviceEnumerator()
        {
            BindingOperations.EnableCollectionSynchronization(VideoDevices, _lock);
            EnumerateVideoDevices();

            var creationQuery =
                "SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.Description LIKE '%USB Video Device%'";
            var deletionQuery =
                "SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.Description LIKE '%USB Video Device%'";

            var scope = new ManagementScope(@"\\.\root\cimv2");

            _creationWatcher = new ManagementEventWatcher(scope, new EventQuery(creationQuery));
            _deletionWatcher = new ManagementEventWatcher(scope, new EventQuery(deletionQuery));

            _creationWatcher.EventArrived += UsbCameraAdded;
            _deletionWatcher.EventArrived += UsbCameraRemoved;

            _creationWatcher.Start();
            _deletionWatcher.Start();
        }

        public ObservableCollection<VideoDevice> VideoDevices { get; } = new ObservableCollection<VideoDevice>();

        public void Dispose()
        {
            _creationWatcher?.Stop();
            _deletionWatcher?.Stop();

            _creationWatcher?.Dispose();
            _deletionWatcher?.Dispose();
        }

        private void UsbCameraAdded(object sender, EventArrivedEventArgs e)
        {
            var eventProps = e.NewEvent.Properties;
            var targetInstance = eventProps["TargetInstance"];
            var value = (ManagementBaseObject)targetInstance.Value;
            var props = value.Properties;

            var friendlyName = props["Caption"].Value.ToString();
            var deviceId = props["DeviceID"].Value.ToString()?.Replace("\\", "#").ToLower();
            var symbolicLink = $"\\\\?\\{deviceId}#{{{KSCATEGORY_VIDEO_CAMERA}}}\\global";

            Debug.WriteLine($"USB camera added: {friendlyName}");

            lock (_lock)
            {
                VideoDevices.Add(new VideoDevice(friendlyName, symbolicLink));
            }
        }

        private void UsbCameraRemoved(object sender, EventArrivedEventArgs e)
        {
            var eventProps = e.NewEvent.Properties;
            var targetInstance = eventProps["TargetInstance"];
            var value = (ManagementBaseObject)targetInstance.Value;
            var props = value.Properties;
            var friendlyName = props["Caption"].Value.ToString();
            var deviceId = props["DeviceID"].Value.ToString()?.Replace("\\", "#").ToLower();

            Debug.WriteLine($"USB camera removed: {friendlyName}");

            lock (_lock)
            {
                VideoDevices.Remove(VideoDevices.First(vd => vd.SymbolicLink.Contains(deviceId)));
            }
        }

        private void EnumerateVideoDevices()
        {
            lock (_lock)
            {
                var mfVideoDevices = MediaFactory.MFEnumVideoDeviceSources();
                var videoDevices = mfVideoDevices.Select(videoDevice =>
                    new VideoDevice(videoDevice.FriendlyName, videoDevice.SymbolicLink));
                foreach (var videoDevice in videoDevices)
                {
                    VideoDevices.Add(videoDevice);
                }
            }
        }
    }
}