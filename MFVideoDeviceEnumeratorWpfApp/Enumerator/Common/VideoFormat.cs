using System;
using System.Reflection;
using Vortice.MediaFoundation;

namespace MFVideoDeviceEnumeratorWpfApp.Enumerator.Common
{
    public class VideoFormat : IVideoFormat
    {
        private static readonly Guid MfMediaTypeVideo =
            new Guid(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        public VideoFormat(string videoDeviceName, Guid majorType, Guid subType, int frameSizeWidth,
            int frameSizeHeight, int frameRate,
            int frameRateDenominator)
        {
            DeviceFriendlyName = videoDeviceName;
            MajorType = MfMediaTypeVideo == majorType ? "Video" : "Unknown";
            SubType = GetPropertyName(subType);
            FrameSizeWidth = frameSizeWidth;
            FrameSizeHeight = frameSizeHeight;
            FrameRate = frameRate / frameRateDenominator;
            Uri =
                $"device://dshow?video={DeviceFriendlyName}&video_size={FrameSizeWidth}x{FrameSizeHeight}&framerate={FrameRate}";
        }

        public VideoFormat(string videoDeviceName, string majorType, string subType, int frameSizeWidth,
            int frameSizeHeight, int frameRate,
            int frameRateDenominator)
        {
            DeviceFriendlyName = videoDeviceName;
            MajorType = majorType;
            SubType = subType;
            FrameSizeWidth = frameSizeWidth;
            FrameSizeHeight = frameSizeHeight;
            FrameRate = frameRate / frameRateDenominator;
            Uri =
                $"device://dshow?video={DeviceFriendlyName}&video_size={FrameSizeWidth}x{FrameSizeHeight}&framerate={FrameRate}";
        }

        public string DeviceFriendlyName { get; }
        public string MajorType { get; }
        public string SubType { get; }
        public int FrameSizeWidth { get; }
        public int FrameSizeHeight { get; }
        public int FrameRate { get; }
        public string Uri { get; }

        public override string ToString()
        {
            return $"{SubType}, {FrameSizeWidth}x{FrameSizeHeight}, {FrameRate}FPS";
        }

        private static string GetPropertyName(Guid guid)
        {
            var type = typeof(VideoFormatGuids);
            foreach (var property in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                if (property.FieldType == typeof(Guid))
                {
                    var temp = property.GetValue(null);
                    if (temp is Guid value)
                        if (value == guid)
                            return property.Name.ToUpper();
                }

            return null; // not found
        }
    }
}