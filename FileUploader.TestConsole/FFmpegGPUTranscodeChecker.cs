using System.Diagnostics;
using System.Text.RegularExpressions;

class FFmpegGPUTranscodeChecker
{
    public static (bool hasGPU, string gpuName) CheckFFmpegGPUSupport()
    {
        var output = RunFFmpegCommand("-hide_banner -hwaccels");
        string[] gpuAccels = { "cuda", "nvenc", "qsv", "vaapi", "vdpau", "videotoolbox", "d3d11va", "dxva2" };
        var supportedAccels = gpuAccels.Where(accel => output.Contains(accel)).ToList();

        return (supportedAccels.Any(), string.Join(", ", supportedAccels));
    }

    public static (bool canEncode, string codecs) CheckGPUEncodingSupport()
    {
        var output = RunFFmpegCommand("-hide_banner -encoders");
        var gpuCodecs = new Regex(@"^\s*[VAS][\S.]+\s+(.*?(nvenc|qsv|vaapi|vdpau|videotoolbox|amf))",RegexOptions.Multiline)
                            .Matches(output)
                            .Cast<Match>()
                            .Select(m => m.Groups[1].Value)
                            .Distinct()
                            .ToList();

        return (gpuCodecs.Any(), string.Join(", ", gpuCodecs));
    }

    public static string RunFFmpegCommand(string arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }

    public static void Check()
    {
        var (hasGPUSupport, gpuName) = CheckFFmpegGPUSupport();
        if (hasGPUSupport)
        {
            Console.WriteLine($"FFmpeg supports GPU acceleration. Available methods: {gpuName}");

            var (canEncode, codecs) = CheckGPUEncodingSupport();
            if (canEncode)
            {
                Console.WriteLine($"GPU encoding is supported. Available codecs: {codecs}");
            }
            else
            {
                Console.WriteLine("GPU acceleration is supported, but no GPU encoding codecs were found.");
            }
        }
        else
        {
            Console.WriteLine("FFmpeg does not support GPU acceleration on this system.");
        }
    }
}