using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

class GPUTranscodeChecker
{
    public static bool HasGPU()
    {
        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
        {
            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"].ToString().ToLower();
                if (name.Contains("nvidia") || name.Contains("amd") || name.Contains("intel"))
                {
                    return true;
                }
            }
        }
        return false;
    }

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

    public static void Check()
    {
        if (HasGPU())
        {
            Console.WriteLine("GPU detected.");
            if (CanTranscodeWithGPU())
            {
                Console.WriteLine("GPU transcoding is supported.");
            }
            else
            {
                Console.WriteLine("GPU detected, but transcoding may not be supported.");
            }
        }
        else
        {
            Console.WriteLine("No GPU detected.");
        }
    }
}
