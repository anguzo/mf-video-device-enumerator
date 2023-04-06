using System;
using System.Diagnostics;
using System.Management;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.Common;
using SharpGen.Runtime;
using Vortice.MediaFoundation;

namespace MFVideoDeviceEnumeratorWpfApp.Enumerator.WMI
{
    public class WmiVideoDeviceManager : VideoDeviceManager, IDisposable
    {
        public const string Namespace = @"root\cimv2";

        public const string QueryExpression =
            "SELECT * FROM __InstanceOperationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.Description LIKE '%USB Video Device%'";

        private static readonly Guid KsCategoryVideoCamera =
            new Guid(0xe5323777, 0xf976, 0x4f5b, 0x9b, 0x55, 0xb9, 0x46, 0x99, 0xc4, 0x6e, 0x44);

        private readonly ManagementEventWatcher _usbWatcher;

        public WmiVideoDeviceManager()
        {
            EnumerateVideoDevices();

            var creationQuery =
                new WqlEventQuery(
                    QueryExpression);

            var scope = new ManagementScope(Namespace);

            _usbWatcher = new ManagementEventWatcher(scope, creationQuery);

            _usbWatcher.EventArrived += OnUsbCameraEvent;

            _usbWatcher.Start();
        }

        public void Dispose()
        {
            _usbWatcher.EventArrived -= OnUsbCameraEvent;
            _usbWatcher?.Stop();
            _usbWatcher?.Dispose();
        }

        private void OnUsbCameraEvent(object sender, EventArrivedEventArgs eventArgs)
        {
            switch (eventArgs.NewEvent.ClassPath.ClassName)
            {
                case "__InstanceCreationEvent":
                {
                    var targetInstance = eventArgs.NewEvent.Properties["TargetInstance"];
                    var win32PnpProperties = ((ManagementBaseObject)targetInstance.Value).Properties;
                    var friendlyName = win32PnpProperties["Caption"].Value.ToString();
                    var deviceId = win32PnpProperties["DeviceID"].Value.ToString()?.Replace("\\", "#").ToLower();
                    var symbolicLink = $"\\\\?\\{deviceId}#{{{KsCategoryVideoCamera}}}\\global";
                    try
                    {
                        Add(new MiVideoDevice(friendlyName, symbolicLink));
                    }
                    catch (SharpGenException e)
                    {
                        Debug.WriteLine(e);
                    }

                    break;
                }
                case "__InstanceDeletionEvent":
                {
                    var targetInstance = eventArgs.NewEvent.Properties["TargetInstance"];
                    var win32PnpProperties = ((ManagementBaseObject)targetInstance.Value).Properties;
                    var deviceId = win32PnpProperties["DeviceID"].Value.ToString()?.Replace("\\", "#").ToLower();
                    var symbolicLink = $"\\\\?\\{deviceId}#{{{KsCategoryVideoCamera}}}\\global";
                    try
                    {
                        Remove(symbolicLink);
                    }
                    catch (InvalidOperationException e)
                    {
                        Debug.WriteLine(e);
                    }
                    catch (SharpGenException e)
                    {
                        Debug.WriteLine(e);
                    }

                    break;
                }
            }
        }

        private void EnumerateVideoDevices()
        {
            var mfVideoDevices = MediaFactory.MFEnumVideoDeviceSources();
            foreach (var mfVideoDevice in mfVideoDevices)
                Add(new MiVideoDevice(mfVideoDevice.FriendlyName,
                    mfVideoDevice.SymbolicLink));
        }
    }
}