using FileUploader.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FileUploader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "..\\Uploads");

        public FileUploadController()
        {
            if (!Directory.Exists(_uploadFolder))
                Directory.CreateDirectory(_uploadFolder);
        }

        [HttpPost("upload-full")]
        public async Task<IActionResult> Upload([FromForm] FileUpload uploadedFile)
        {
            if (!Directory.Exists(_uploadFolder))
                Directory.CreateDirectory(_uploadFolder);

            string filePath = Path.Combine(_uploadFolder, uploadedFile.fileName);

            try
            {
                // Append chunk to file
                using (var stream = new FileStream(filePath, uploadedFile.chunkNumber == 1 ? FileMode.Create : FileMode.Append, FileAccess.Write))
                {
                    await uploadedFile.chunk.CopyToAsync(stream);
                }

                Console.WriteLine("totalChunks: " + uploadedFile.totalChunks);
                Console.WriteLine("chunkNumber: " + uploadedFile.chunkNumber);
                Console.WriteLine("chunkSize: " + (((uploadedFile.chunk.Length / 1024) / 1024)) + " MB");

                // Check if all chunks have been received
                if (uploadedFile.chunkNumber == uploadedFile.totalChunks)
                {
                    Console.Clear();
                    return Ok(new { message = "File upload completed successfully." });
                }

                return Ok(new { message = "Chunk uploaded successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost("upload-chunk")]
        public async Task<IActionResult> UploadChunk([FromForm] FileUpload uploadedFile)
        {
            if (!Directory.Exists(_uploadFolder))
                Directory.CreateDirectory(_uploadFolder);

            string fileExtension = Path.GetExtension(uploadedFile.fileName);
            uploadedFile.fileName = uploadedFile.fileName.Trim(fileExtension.ToCharArray());

            // Determine the length of the string based on the total number of chunks
            int totalLength = uploadedFile.totalChunks.ToString().Length;

            // Generate the zero-padded chunk string
            string paddedChunkNumber = uploadedFile.chunkNumber.ToString().PadLeft(totalLength, '0');


            string filePath = Path.Combine(_uploadFolder, $"{uploadedFile.fileName}");
            string tempFilePath = filePath;
            string fileDirectoryName = Path.Combine(Path.GetDirectoryName(filePath) ?? "", tempFilePath);
            if (!Directory.Exists(fileDirectoryName))
                Directory.CreateDirectory(fileDirectoryName);

            filePath = Path.Combine(fileDirectoryName, $"{uploadedFile.fileName}~{paddedChunkNumber}{fileExtension}");

            try
            {

                // Append chunk to file
                using (var stream = new FileStream(filePath, uploadedFile.chunkNumber == 1 ? FileMode.Create : FileMode.Append, FileAccess.Write))
                {
                    await uploadedFile.chunk.CopyToAsync(stream);
                }

                Console.WriteLine("chunkNumber: " + paddedChunkNumber);

                // Check if all chunks have been received
                if (uploadedFile.chunkNumber == uploadedFile.totalChunks)
                {
                    string combinedFilePath = Path.Combine(_uploadFolder, $"{uploadedFile.fileName}{fileExtension}");

                    //Task.Run(async () =>
                    //{
                    //    await CombineFilesAsync(filePath, combinedFilePath);
                    //});


                    await CombineFilesAsync(filePath, combinedFilePath);

                    var inputFileName = combinedFilePath;
                    var commandName = "ffmpeg";

                    //var presetType = "ultrafast";

                    //var lowResolution = "360:640";
                    //var lowAudioBitrate = "64K";
                    //var lowVideoBitrate = "250k";

                    //List<string> lowQualityCommand = [
                    //    "-i", inputFileName,
                    //    "-filter:v", $"scale={lowResolution}",
                    //    "-c:a", "aac", "-ar", "48000","-b:a", lowAudioBitrate,
                    //    "-c:v", "libx264", "-b:v", lowVideoBitrate, 
                    //    "-preset", presetType,
                    //    "-g", "48", "-keyint_min", "48",
                    //    "-sc_threshold", "0", "-threads", "4",
                    //    "-hls_time", "10", "-hls_segment_filename", "360p_%03d.ts", "-hls_playlist_type", "vod",
                    //    "-hls_flags", "independent_segments", "-f", "hls", "360p.m3u8"
                    //];

                    //var hdResolution = "720:1280";
                    //var hdAudioBitrate = "96k";
                    //var hdVideoBitrate = "250k";

                    //List<string> hdCommand = [
                    //    "-i", "inputs.mp4",
                    //    "-filter:v", $"scale={hdResolution}",
                    //    "-c:a", "aac", "-ar", "48000", "-b:a", hdAudioBitrate,
                    //    "-c:v", "libx264", "-b:v", hdVideoBitrate,
                    //    "-preset", presetType,
                    //    "-g", "48", "-keyint_min", "48",
                    //    "-sc_threshold", "0", "-hls_time", "10",
                    //    "-hls_segment_filename", "720p_%03d.ts", "-hls_playlist_type", "vod",
                    //    "-hls_flags", "independent_segments", "-f", "hls", "720p.m3u8"
                    //];

                    //var fullHdResolution = "1080:1920";
                    //var fullHdAudioBitrate = "128k";
                    //var fullHdVideoBitrate = "1500k";

                    //List<string> fullHdCommand = [
                    //    "-i", "inputs.mp4",
                    //    "-filter:v", $"scale={fullHdResolution}",
                    //    "-c:a", "aac", "-ar", "48000", "-b:a", fullHdAudioBitrate,
                    //    "-c:v", "libx264", "-b:v", fullHdVideoBitrate, 
                    //    "-preset",presetType,
                    //    "-g", "48", "-keyint_min", "48", 
                    //    "-sc_threshold", "0", "-hls_time", "10",
                    //    "-hls_segment_filename", "1080p_%03d.ts", "-hls_playlist_type", "vod",
                    //    "-hls_flags", "independent_segments", "-f", "hls", "1080p.m3u8"
                    //];

                    //List<List<string>> commands = new List<List<string>>()
                    //{
                    //    lowQualityCommand,
                    //    hdCommand,
                    //    fullHdCommand
                    //};

                    List<string> commandArgs = ["-filter:v:0", "scale=360:640",
"-c:a",
"aac",
"-ar",
"48000",
"-b:a",
"64k",
"-c:v:0",
"libx264",
"-b:v:0",
"250k",
"-preset",
"ultrafast",
"-g",
"48",
"-keyint_min",
"48",
"-sc_threshold",
"0",
"-hls_time",
"10",
"-hls_segment_filename",
"360p_%03d.ts",
"-hls_playlist_type",
"vod",
"-hls_flags",
"independent_segments",
"-filter:v:1",
"scale=720:1280",
"-c:a",
"aac",
"-ar",
"48000",
"-b:a",
"96k",
"-c:v:1",
"libx264",
"-b:v:1",
"500k",
"-preset",
"ultrafast",
"-g",
"48",
"-keyint_min",
"48",
"-sc_threshold",
"0",
"-hls_time",
"10",
"-hls_segment_filename",
"720p_%03d.ts",
"-hls_playlist_type",
"vod",
"-hls_flags",
"independent_segments",
"-filter:v:2",
"scale=1080:1920",
"-c:a",
"aac",
"-ar",
"48000",
"-b:a",
"128k",
"-c:v:2",
"libx264",
"-b:v:2",
"1500k",
"-preset",
"ultrafast",
"-g",
"48",
"-keyint_min",
"48",
"-sc_threshold",
"0",
"-hls_time",
"10",
"-hls_segment_filename",
"1080p_%03d.ts",
"-hls_playlist_type",
"vod",
"-hls_flags",
"independent_segments",
"-master_pl_name",
"master.m3u8",
"-hls_list_size",
"0",
"-f",
"hls",
"master.m3u8"];

                    var finalCommandArgs = $"-i {inputFileName} {string.Join(" ", commandArgs)}";

                    System.IO.File.WriteAllText(Path.Combine(_uploadFolder, "master.m3u8"), "");
                    await RunCommandAsync(commandName, string.Join(" ", commandArgs));

                    //foreach (var command in commands)
                    //{
                    //    await RunCommandAsync(commandName, string.Join(" ", command));
                    //}


                    List<string> masterFileText = [
                        "#EXTM3U" ,
                        "#EXT-X-VERSION:3",
                        "#EXT-X-STREAM-INF:BANDWIDTH=350000,RESOLUTION=360x640",
                        "360p.m3u8",
                        "#EXT-X-STREAM-INF:BANDWIDTH=800000,RESOLUTION=720x1280",
                        "720p.m3u8",
                        "#EXT-X-STREAM-INF:BANDWIDTH=1500000,RESOLUTION=1080x1920",
                        "1080p.m3u8"
                    ];

                    System.IO.File.WriteAllText(Path.Combine(_uploadFolder, "master.m3u8"), string.Join("\n", masterFileText));

                    Console.Clear();
                    return Ok(new { message = "File upload completed successfully." });
                }

                return Ok(new { message = "Chunk uploaded successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        private async Task RunCommandAsync(string command, string args)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();

                // Optionally read the output (stdout and stderr)
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg error: {error}");
                }
            }
        }

        private async Task CombineFilesAsync(string filePath, string outputFilePath)
        {
            // Get all the files in the directory
            string[] files = Directory.GetFiles(Path.GetDirectoryName(filePath));
            files = files.OrderBy(x => x).ToArray();

            // Create the output file stream
            using (FileStream outputStream = System.IO.File.Create(outputFilePath))
            {
                // Combine the contents of all the files into the output stream asynchronously
                foreach (string file in files)
                {
                    using (FileStream inputStream = System.IO.File.OpenRead(file))
                    {
                        await inputStream.CopyToAsync(outputStream);
                    }
                }
            }

            if (Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.Delete(Path.GetDirectoryName(filePath), true);
            }
        }
    }
}
