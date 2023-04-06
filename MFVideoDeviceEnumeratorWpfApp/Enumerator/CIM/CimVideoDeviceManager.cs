using System;
using System.Diagnostics;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.Common;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using SharpGen.Runtime;
using Vortice.MediaFoundation;

namespace MFVideoDeviceEnumeratorWpfApp.Enumerator.CIM
{
    public class CimVideoDeviceManager : VideoDeviceManager, IDisposable
    {
        public const string Namespace = @"root\cimv2";
        public const string QueryDialect = "WQL";

        public const string QueryExpression =
            "SELECT * FROM __InstanceOperationEvent  WITHIN 1 WHERE TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.Description LIKE '%USB Video Device%'";

        private static readonly Guid KsCategoryVideoCamera =
            new Guid(0xe5323777, 0xf976, 0x4f5b, 0x9b, 0x55, 0xb9, 0x46, 0x99, 0xc4, 0x6e, 0x44);

        private readonly CimSession _cimSession;

        public CimVideoDeviceManager()
        {
            EnumerateVideoDevices();

            var options = new DComSessionOptions { Impersonation = ImpersonationType.Impersonate };
            _cimSession = CimSession.Create(Environment.MachineName, options);

            var cimSubscriptionResultsCreation = _cimSession.SubscribeAsync(Namespace, QueryDialect, QueryExpression);

            var creationWatcher = new UsbObserver(Add, Remove);

            cimSubscriptionResultsCreation.Subscribe(creationWatcher);
        }

        public void Dispose()
        {
            _cimSession?.Close();
            _cimSession?.Dispose();
        }

        private void EnumerateVideoDevices()
        {
            var mfVideoDevices = MediaFactory.MFEnumVideoDeviceSources();
            foreach (var mfVideoDevice in mfVideoDevices)
                Add(new MiVideoDevice(mfVideoDevice.FriendlyName,
                    mfVideoDevice.SymbolicLink));
        }

        private class UsbObserver : IObserver<CimSubscriptionResult>
        {
            private readonly Action<IVideoDevice> _addDevice;
            private readonly Action<string> _removeDevice;

            public UsbObserver(Action<IVideoDevice> addDevice, Action<string> removeDevice)
            {
                _addDevice = addDevice;
                _removeDevice = removeDevice;
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
                            _addDevice(new MiVideoDevice(friendlyName, symbolicLink));
                        }
                        catch (SharpGenException e)
                        {
                            Debug.WriteLine(e);
                        }

                        break;
                    }
                    case "__InstanceDeletionEvent":
                    {
                        var targetInstance = value.Instance.CimInstanceProperties["TargetInstance"];
                        var win32PnpProperties = ((CimInstance)targetInstance.Value).CimInstanceProperties;
                        var deviceId = win32PnpProperties["DeviceID"].Value.ToString()?.Replace("\\", "#").ToLower();
                        var symbolicLink = $"\\\\?\\{deviceId}#{{{KsCategoryVideoCamera}}}\\global";
                        try
                        {
                            _removeDevice(symbolicLink);
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
        }
    }
}