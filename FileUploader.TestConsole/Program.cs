using System.Diagnostics;

namespace FileUploader.TestConsole
{

    public class Program
    {
        //public static void Main(string[] args)
        //{
        //    Console.WriteLine("Hello, World!");


        //    Console.WriteLine("GPUTranscodeChecker.Check();");
        //    GPUTranscodeChecker.Check();
        //    Console.WriteLine("==================================");

        //    Console.WriteLine("FFmpegGPUTranscodeChecker.Check();");
        //    FFmpegGPUTranscodeChecker.Check();
        //    Console.WriteLine("==================================");

        //    Console.WriteLine("HlsTranscoder.GetTranscoder();");
        //    Console.WriteLine(HlsTranscoder.GetTranscoder());
        //    Console.WriteLine("==================================");
        //}



        static void Main(string[] args)
        {
            int originalWidth = 1920;  // Example: Original video width
            int originalHeight = 800;  // Example: Original video height

            // Target heights (144p, 240p, 360p, 480p, 720p, 1080p, 1440p, 2160p, 4320p)
            int[] targetHeights = { 144, 240, 360, 480, 720, 1080, 1440, 2160, 4320 };

            // Compute aspect ratio
            double aspectRatio = (double)originalWidth / originalHeight;

            bool originalResolutionProcessed = false;

            // Loop through target resolutions and generate M3U8 if within bounds
            foreach (int targetHeight in targetHeights)
            {
                if (targetHeight > originalHeight) break;  // Stop if the target is larger than the original

                // Calculate target width based on aspect ratio
                int targetWidth = (int)(targetHeight * aspectRatio);

                // Generate the FFmpeg command for the resolution
                GenerateM3U8File(targetWidth, targetHeight, "input.mp4", $"output_{targetHeight}p.m3u8");

                // Check if we've hit the original resolution
                if (targetHeight == originalHeight && targetWidth == originalWidth)
                {
                    originalResolutionProcessed = true;
                }
            }

            // If the original resolution wasn't processed, generate it explicitly
            if (!originalResolutionProcessed)
            {
                GenerateM3U8File(originalWidth, originalHeight, "input.mp4", $"output_{originalHeight}p_original.m3u8");
            }
        }

        // Method to run FFmpeg to generate M3U8 file
        static void GenerateM3U8File(int width, int height, string inputFile, string outputFile)
        {
            string ffmpegPath = @"path_to_ffmpeg\ffmpeg.exe";  // Set the correct path to ffmpeg.exe

            // FFmpeg command to generate HLS (m3u8) with specific resolution
            string arguments = $"-i {inputFile} -vf scale={width}:{height} -c:a copy -hls_time 10 -hls_list_size 0 -f hls {outputFile}";

            // You can start the process here or for now, just print the command for debugging
            Console.WriteLine($"Resolution: {width}x{height}");
        }
    }
}
