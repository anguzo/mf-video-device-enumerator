using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace MFVideoDeviceEnumerator.WinRT
{
    public class VideoDevice
    {
        private VideoDevice(string friendlyName, string symbolicLink, Guid sourceType, IEnumerable<VideoFormat> formats)
        {
            FriendlyName = friendlyName;
            SymbolicLink = symbolicLink;
            SourceType = sourceType;
            Formats = formats;
        }

        public string FriendlyName { get; set; }
        public string SymbolicLink { get; }
        public Guid SourceType { get; set; }
        public IEnumerable<VideoFormat> Formats { get; set; }
        public VideoFormat DefaultFormat => Formats.Where(f => f.FrameRate >= 30).MaxBy(f => f.FrameSizeHeight);

        public static async Task<VideoDevice> CreateInstance(string friendlyName, string symbolicLink, Guid sourceType)
        {
            var formats = new List<VideoFormat>();

            var mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                VideoDeviceId = symbolicLink,
                MediaCategory = MediaCategory.Media
            });

            if (mediaCapture.VideoDeviceController != null)
            {
                var properties =
                    mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);

                foreach (var property in properties)
                {
                    var videoEncodingProperties = (VideoEncodingProperties)property;
                    formats.Add(new VideoFormat(videoEncodingProperties.Type, videoEncodingProperties.Subtype,
                        (int)videoEncodingProperties.Width,
                        (int)videoEncodingProperties.Height, (int)videoEncodingProperties.FrameRate.Numerator,
                        (int)videoEncodingProperties.FrameRate.Denominator));

                    await mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview,
                        videoEncodingProperties);

                    var encodingProfile = ImageEncodingProperties.CreatePng();

                    var snapshotFolder = await StorageFolder.GetFolderFromPathAsync("C:\\Users\\Andrei\\Downloads\\photos");
                    var snapshotFile =
                        await snapshotFolder.CreateFileAsync("Image.png", CreationCollisionOption.GenerateUniqueName);

                    // await mediaCapture.CapturePhotoToStorageFileAsync(encodingProfile, snapshotFile);
                }
            }

            return new VideoDevice(friendlyName, symbolicLink, sourceType, formats);
        }

        public override string ToString()
        {
            return $"{FriendlyName} ({DefaultFormat})";
        }
    }
}