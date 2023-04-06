using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.Common;

namespace MFVideoDeviceEnumeratorWpfApp.Enumerator.WinRT
{
    public class WinRtVideoDevice : VideoDevice
    {
        private WinRtVideoDevice(string friendlyName, string symbolicLink, IEnumerable<IVideoFormat> formats) : base(
            friendlyName, symbolicLink, formats)
        {
        }

        public static async Task<WinRtVideoDevice> CreateInstanceAsync(string friendlyName, string symbolicLink)
        {
            var formats = new List<IVideoFormat>();

            using (var mediaCapture = new MediaCapture())
            {
                await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = symbolicLink,
                    MediaCategory = MediaCategory.Media
                });

                if (mediaCapture.VideoDeviceController != null)
                {
                    var properties = mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(
                        MediaStreamType
                            .VideoPreview);

                    foreach (var property in properties)
                    {
                        var videoEncodingProperties = (VideoEncodingProperties)property;
                        formats.Add(new VideoFormat(friendlyName, videoEncodingProperties.Type,
                            videoEncodingProperties.Subtype,
                            (int)videoEncodingProperties.Width,
                            (int)videoEncodingProperties.Height, (int)videoEncodingProperties.FrameRate.Numerator,
                            (int)videoEncodingProperties.FrameRate.Denominator));
                    }
                }

                return new WinRtVideoDevice(friendlyName, symbolicLink, formats);
            }
        }
    }
}