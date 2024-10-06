namespace FileUploader.TestConsole
{

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");


            Console.WriteLine("GPUTranscodeChecker.Check();");
            GPUTranscodeChecker.Check();
            Console.WriteLine("==================================");

            Console.WriteLine("FFmpegGPUTranscodeChecker.Check();");
            FFmpegGPUTranscodeChecker.Check();
            Console.WriteLine("==================================");

            Console.WriteLine("HlsTranscoder.GetTranscoder();");
            Console.WriteLine(HlsTranscoder.GetTranscoder());
            Console.WriteLine("==================================");
        }
    }
}
