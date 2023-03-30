using System;
using System.Collections.Generic;
using System.Linq;
using Vortice.MediaFoundation;

namespace MFVideoDeviceEnumerator
{
    public class VideoDevice
    {
        public VideoDevice(string friendlyName, string symbolicLink)
        {
            FriendlyName = friendlyName;
            SymbolicLink = symbolicLink;
            SourceType = CaptureDeviceAttributeKeys.SourceTypeVidcap;
            Formats = VideoFormatsViaMediaSource.GetVideoFormatsForVideoDevice(this);
        }

        public string FriendlyName { get; set; }
        public string SymbolicLink { get; }
        public Guid SourceType { get; set; }
        public IEnumerable<VideoFormat> Formats { get; set; }

        public VideoFormat DefaultVideoFormat => Formats.Where(f => f.SubType.Contains("MJPG") && f.FrameRate >= 30)
            .OrderByDescending(f => f.FrameSizeHeight).FirstOrDefault();

        public VideoFormat DefaultSnapshotFormat
        {
            get
            {
                var yuyFormat = Formats.Where(f => f.SubType.Contains("YUY")).OrderByDescending(f => f.FrameSizeHeight).FirstOrDefault();
                if (yuyFormat != null)
                    return yuyFormat;
                return Formats.Where(f => f.SubType.Contains("MJPG")).OrderByDescending(f => f.FrameSizeHeight).FirstOrDefault();
            }
        }

        public override string ToString()
        {
            return $"{FriendlyName} (Video: {DefaultVideoFormat}) (Snapshot: {DefaultSnapshotFormat})";
        }
    }
}