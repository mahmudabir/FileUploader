using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUploader.TestConsole
{
    public static class HlsTranscoder
    {
        public static GpuType? DeviceGpuType;

        // Method to detect which GPU is available
        public static GpuType DetectGpuType()
        {
            //ffmpeg 
            //-hide_banner -hwaccels => Hardware Accelerators
            //-hide_banner -encoders => Video Encoders
            //-hide_banner -decoders => Video Decoders (Used for transcoding)

            Process process = new Process();
            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.Arguments = "-hide_banner -decoders";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Check the output for specific hardware encoders
            if (output.Contains("amf"))
                return GpuType.AMD;
            if (output.Contains("cuda") || output.Contains("nvenc"))
                return GpuType.Nvidia;
            if (output.Contains("qsv"))
                return GpuType.Intel;

            return GpuType.None;
        }

        public static string GetTranscoder()
        {
            GpuType gpuType = DetectGpuType();

            var codec = gpuType switch
            {
                GpuType.AMD => "h264_amf",
                GpuType.Nvidia => "h264_nvenc",
                GpuType.Intel => "h264_qsv",
                _ => "libx264"
            };

            return codec;
        }
    }

    public enum GpuType
    {
        None,
        AMD,
        Nvidia,
        Intel
    }
}
