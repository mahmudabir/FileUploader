using FileUploader.Models;

using System.Diagnostics;
using System.Management;

namespace FileUploader.Services
{
    public static class FFmpegHelper
    {
        public static bool CanTranscodeWithGPU()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-hwaccels",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                         .Any(line => line.Contains("cuda") || line.Contains("dxva2") || line.Contains("qsv") || line.Contains("d3d11va"));
        }

        // Method to detect which GPU is available
        public static GpuType DetectGpuType()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                if (searcher?.Get() == null) return GpuType.None;
                foreach (var obj in searcher.Get())
                {
                    var name = obj["Name"]?.ToString()?.ToLower();

                    if (string.IsNullOrEmpty(name)) return GpuType.None;
                    else if (name.Contains("nvidia")) return GpuType.Nvidia;
                    else if (name.Contains("amd")) return GpuType.AMD;
                    else if (name.Contains("intel")) return GpuType.Intel;
                    else return GpuType.None;
                }
                return GpuType.None;
            }
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
