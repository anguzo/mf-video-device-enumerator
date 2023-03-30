using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.Common;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using SharpGen.Runtime;
using Vortice.MediaFoundation;

namespace MFVideoDeviceEnumeratorWpfApp.Enumerator.CIM
{
    public class VideoDeviceEnumerator : IDisposable
    {
        public const string Namespace = @"root\cimv2";
        public const string QueryDialect = "WQL";

        public const string QueryExpression =
            "SELECT * FROM __InstanceOperationEvent  WITHIN 1 WHERE TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.Description LIKE '%USB Video Device%'";

        private static readonly Guid KsCategoryVideoCamera =
            new Guid(0xe5323777, 0xf976, 0x4f5b, 0x9b, 0x55, 0xb9, 0x46, 0x99, 0xc4, 0x6e, 0x44);

        private readonly CimSession _cimSession;

        public VideoDeviceEnumerator()
        {
            BindingOperations.EnableCollectionSynchronization(VideoDevices, new object());
            EnumerateVideoDevices();

            var options = new DComSessionOptions { Impersonation = ImpersonationType.Impersonate };
            _cimSession = CimSession.Create(Environment.MachineName, options);

            var cimSubscriptionResultsCreation = _cimSession.SubscribeAsync(Namespace, QueryDialect, QueryExpression);

            var creationWatcher = new UsbObserver(VideoDevices);

            cimSubscriptionResultsCreation.Subscribe(creationWatcher);
        }

        public ObservableCollection<IVideoDevice> VideoDevices { get; } =
            new ObservableCollection<IVideoDevice>();

        public void Dispose()
        {
            _cimSession?.Close();
            _cimSession?.Dispose();
        }

        private void EnumerateVideoDevices()
        {
            var mfVideoDevices = MediaFactory.MFEnumVideoDeviceSources();
            foreach (var mfVideoDevice in mfVideoDevices)
                VideoDevices.Add(new MiVideoDevice(mfVideoDevice.FriendlyName,
                    mfVideoDevice.SymbolicLink));
        }

        private class UsbObserver : IObserver<CimSubscriptionResult>
        {
            private readonly ObservableCollection<IVideoDevice> _videoDevices;

            public UsbObserver(ObservableCollection<IVideoDevice> videoDevices)
            {
                _videoDevices = videoDevices;
            }

            public void OnCompleted()
            {
                Debug.WriteLine("Done");
            }

            public void OnError(Exception e)
            {
                Debug.WriteLine("Error: " + e.Message);
            }

            public void OnNext(CimSubscriptionResult value)
            {
                switch (value.Instance.CimSystemProperties.ClassName)
                {
                    case "__InstanceCreationEvent":
                    {
                        var targetInstance = value.Instance.CimInstanceProperties["TargetInstance"];
                        var win32PnpProperties = ((CimInstance)targetInstance.Value).CimInstanceProperties;
                        var friendlyName = win32PnpProperties["Caption"].Value.ToString();
                        var deviceId = win32PnpProperties["DeviceID"].Value.ToString()?.Replace("\\", "#").ToLower();
                        var symbolicLink = $"\\\\?\\{deviceId}#{{{KsCategoryVideoCamera}}}\\global";
                        try
                        {
                            _videoDevices.Add(new MiVideoDevice(friendlyName, symbolicLink));
                        }
                        catch (SharpGenException)
                        {
                        }

                        break;
                    }
                    case "__InstanceDeletionEvent":
                    {
                        var targetInstance = value.Instance.CimInstanceProperties["TargetInstance"];
                        var win32PnpProperties = ((CimInstance)targetInstance.Value).CimInstanceProperties;
                        var deviceId = win32PnpProperties["DeviceID"].Value.ToString()?.Replace("\\", "#").ToLower();
                        try
                        {
                            _videoDevices.Remove(_videoDevices.First(vd => vd.SymbolicLink.Contains(deviceId)));
                        }
                        catch (InvalidOperationException)
                        {
                        }
                        catch (SharpGenException)
                        {
                        }

                        break;
                    }
                }
            }
        }
    }
}