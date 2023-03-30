using System;
using System.Collections.Generic;
using Vortice.MediaFoundation;

namespace MFVideoDeviceEnumerator
{
    public static class VideoFormatsViaMediaSource
    {
        public static readonly Guid MfMediaTypeVideo = new Guid("{73646976-0000-0010-8000-00AA00389B71}");

        public static IEnumerable<VideoFormat> GetVideoFormatsForVideoDevice(VideoDevice videoDevice)
        {
            var formatList = new List<VideoFormat>();

            using (var mediaSource = GetMediaSourceFromVideoDevice(videoDevice))
            {
                using (var sourcePresentationDescriptor = mediaSource.CreatePresentationDescriptor())
                {
                    var sourceStreamCount = sourcePresentationDescriptor.StreamDescriptorCount;

                    for (var i = 0; i < sourceStreamCount; i++)
                    {
                        var guidMajorType =
                            GetMajorMediaTypeFromPresentationDescriptor(sourcePresentationDescriptor, i);
                        if (guidMajorType != MfMediaTypeVideo) continue;

                        sourcePresentationDescriptor.GetStreamDescriptorByIndex(i, out var streamIsSelected,
                            out var videoStreamDescriptor);

                        using (videoStreamDescriptor)
                        {
                            if (streamIsSelected == false) continue;

                            using (var typeHandler = videoStreamDescriptor.MediaTypeHandler)
                            {
                                var mediaTypeCount = typeHandler.MediaTypeCount;

                                for (var mediaTypeId = 0; mediaTypeId < mediaTypeCount; mediaTypeId++)
                                    using (var workingMediaType = typeHandler.GetMediaTypeByIndex(mediaTypeId))
                                    {
                                        var videoFormat = GetVideoFormatFromMediaType(workingMediaType);
                                        formatList.Add(videoFormat);
                                    }
                            }
                        }
                    }
                }
            }

            return formatList;
        }

        public static IMFMediaSource GetMediaSourceFromVideoDevice(VideoDevice videoDevice)
        {
            using (var attributeContainer = MediaFactory.MFCreateAttributes(2))
            {
                attributeContainer.Set(CaptureDeviceAttributeKeys.SourceType, videoDevice.SourceType);

                attributeContainer.Set(CaptureDeviceAttributeKeys.SourceTypeVidcapSymbolicLink,
                    videoDevice.SymbolicLink);

                return MediaFactory.MFCreateDeviceSource(attributeContainer);
            }
        }

        public static Guid GetMajorMediaTypeFromPresentationDescriptor(IMFPresentationDescriptor presentationDescriptor,
            int streamIndex)
        {
            presentationDescriptor.GetStreamDescriptorByIndex(streamIndex, out _,
                out var streamDescriptor);

            using (streamDescriptor)
            {
                return GetMajorMediaTypeFromStreamDescriptor(streamDescriptor);
            }
        }

        public static Guid GetMajorMediaTypeFromStreamDescriptor(IMFStreamDescriptor streamDescriptor)
        {
            using (var pHandler = streamDescriptor.MediaTypeHandler)
            {
                var guidMajorType = pHandler.MajorType;

                return guidMajorType;
            }
        }

        public static VideoFormat GetVideoFormatFromMediaType(IMFMediaType mediaType)
        {
            // MF_MT_MAJOR_TYPE
            // Major type GUID, we return this as human readable text
            var majorType = mediaType.MajorType;

            // MF_MT_SUBTYPE
            // Subtype GUID which describes the basic media type, we return this as human readable text
            var subType = mediaType.Get<Guid>(MediaTypeAttributeKeys.Subtype);

            // MF_MT_FRAME_SIZE
            // the Width and height of a video frame, in pixels
            MediaFactory.MFGetAttributeSize(mediaType, MediaTypeAttributeKeys.FrameSize, out var frameSizeWidth,
                out var frameSizeHeight);

            // MF_MT_FRAME_RATE
            // The frame rate is expressed as a ratio.The upper 32 bits of the attribute value contain the numerator and the lower 32 bits contain the denominator. 
            // For example, if the frame rate is 30 frames per second(fps), the ratio is 30 / 1.If the frame rate is 29.97 fps, the ratio is 30,000 / 1001.
            // we report this back to the user as a decimal
            MediaFactory.MFGetAttributeRatio(mediaType, MediaTypeAttributeKeys.FrameRate, out var frameRate,
                out var frameRateDenominator);

            var videoFormat = new VideoFormat(majorType, subType, (int)frameSizeWidth, (int)frameSizeHeight,
                (int)frameRate,
                (int)frameRateDenominator);

            return videoFormat;
        }
    }
}