using System;
using System.Reflection;
using Vortice.MediaFoundation;

namespace MFVideoDeviceEnumerator
{
    public class VideoFormat
    {
        private static readonly Guid MEDIATYPE_Video =
            new Guid(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        public VideoFormat(Guid majorType, Guid subType, int frameSizeWidth, int frameSizeHeight, int frameRate,
            int frameRateDenominator)
        {
            MajorType = MEDIATYPE_Video == majorType ? "Video" : "Unknown";
            SubType = GetPropertyName(subType);
            FrameSizeWidth = frameSizeWidth;
            FrameSizeHeight = frameSizeHeight;
            FrameRate = frameRate / frameRateDenominator;
        }

        public VideoFormat(string majorType, string subType, int frameSizeWidth, int frameSizeHeight, int frameRate,
            int frameRateDenominator)
        {
            MajorType = majorType;
            SubType = subType;
            FrameSizeWidth = frameSizeWidth;
            FrameSizeHeight = frameSizeHeight;
            FrameRate = frameRate / frameRateDenominator;
        }

        public string MajorType { get; }
        public string SubType { get; }
        public int FrameSizeWidth { get; }
        public int FrameSizeHeight { get; }
        public int FrameRate { get; }

        public override string ToString()
        {
            return $"{SubType}, {FrameSizeWidth} x {FrameSizeHeight}, {FrameRate}FPS";
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