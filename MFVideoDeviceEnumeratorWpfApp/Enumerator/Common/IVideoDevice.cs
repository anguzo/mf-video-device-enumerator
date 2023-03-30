﻿using System;
using System.Collections.Generic;

namespace MFVideoDeviceEnumeratorWpfApp.Enumerator.Common
{
    public interface IVideoDevice
    {
        string FriendlyName { get; set; }
        string SymbolicLink { get; }
        Guid SourceType { get; set; }
        IEnumerable<IVideoFormat> Formats { get; set; }
        string DefaultVideoFormatUri { get; }
        string DefaultSnapshotFormatUri { get; }
        string ToString();
    }
}