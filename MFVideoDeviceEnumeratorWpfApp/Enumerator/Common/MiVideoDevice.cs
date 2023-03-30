namespace MFVideoDeviceEnumeratorWpfApp.Enumerator.Common
{
    public class MiVideoDevice : VideoDevice
    {
        public MiVideoDevice(string friendlyName, string symbolicLink) : base(
            friendlyName, symbolicLink,
            VideoFormatsViaMediaSource.GetVideoFormatsForVideoDevice(friendlyName, symbolicLink))
        {
        }
    }
}