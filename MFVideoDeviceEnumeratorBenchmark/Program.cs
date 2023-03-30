using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Running;
using MFVideoDeviceEnumerator;

namespace MFVideoDeviceEnumeratorBenchmark
{
    [MemoryDiagnoser]
    [NativeMemoryProfiler]
    [ThreadingDiagnoser]
    [ExceptionDiagnoser]
    public class MFVideoDeviceEnumerator
    {
        private readonly VideoDeviceEnumerator _videoDeviceEnumerator;

        public MFVideoDeviceEnumerator()
        {
            _videoDeviceEnumerator = new VideoDeviceEnumerator();
        }

        [Benchmark]
        public List<VideoDevice> ViaMediaSource()
        {
            return _videoDeviceEnumerator.GetVideoDevicesViaMediaSource();
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MFVideoDeviceEnumerator>();
        }
    }
}