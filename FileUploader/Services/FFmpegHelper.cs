using FileUploader.Models;
using System.Diagnostics;

namespace FileUploader.Services
{
    public static class FFmpegHelper
    {

        // Method to detect which GPU is available
        public static GpuType DetectGpuType()
        {
            //ffmpeg 
            //-hide_banner -hwaccels => Hardware Accelerators
            //-hide_banner -encoders => Video Encoders
            
            //-hide_banner -decoders | findstr amf => Video Decoders (Used for transcoding)
            // | findstr amf // Check if AMD GPU is available
            // | findstr nvenc // Check if NVIDIA GPU is available
            // | findstr qsv // Check if INTEL GPU is available

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
            if (output.Contains("nvenc"))
                return GpuType.Nvidia;
            if (output.Contains("qsv"))
                return GpuType.Intel;

            return GpuType.None;
        }

        public static string GetTranscoder(GpuType? gpuType = null)
        {
            gpuType = gpuType ?? DetectGpuType();

            var codec = gpuType switch
            {
                GpuType.AMD => "h264_amf",
                GpuType.Nvidia => "h264_nvenc",
                GpuType.Intel => "h264_qsv",
                _ => "h264" //h264,libx264
            };

            return codec;
        }
    }
}
