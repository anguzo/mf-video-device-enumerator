using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.Common;
using MFVideoDeviceEnumeratorWpfApp.Enumerator.WinRT;
using System.Threading.Tasks;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class Enumerator
    {
        private const string FriendlyName =
            "DJU USB CAMERA";

        private const string SymbolicLink =
            "\\\\?\\usb#vid_4232&pid_1301&mi_00#8&67d9335&1&0000#{e5323777-f976-4f5b-9b55-b94699c46e44}\\global";

        [Benchmark]
        public async Task<IVideoDevice> CreateWinRtVideoDevice()
        {
            return await WinRtVideoDevice.CreateInstanceAsync(FriendlyName, SymbolicLink);
        }

        [Benchmark]
        public IVideoDevice CreateMiVideoDevice()
        {
            return new MiVideoDevice(FriendlyName, SymbolicLink);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Enumerator>();
        }
    }
}