using FileUploader.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace FileUploader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamingController : ControllerBase
    {
        private readonly string videoPath = Path.Combine(Directory.GetCurrentDirectory(), "..\\Uploads");
        private readonly string ffmpegPath = "ffmpeg.exe"; // Path to your FFmpeg executable
        private readonly string ffprobePath = "ffprobe.exe"; // Path to your FFprobe executable
        private const double SegmentDuration = 10; // Segment duration in seconds

        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions;

        public StreamingController(IMemoryCache cache)
        {
            _cache = cache;
            _cacheEntryOptions = new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };
        }

        [HttpGet("clear-cache")]
        public IActionResult ClearAllCache()
        {
            if (_cache is MemoryCache cache)
            {
                cache.Clear();
            }
            return Ok();
        }

        // Serve the master playlist with different quality levels
        [HttpGet("master.m3u8")]
        public async Task<IActionResult> GetMasterPlaylist([FromQuery] string file)
        {
            string _apiBaseUrl = $"{Request.Scheme}://{Request.Host}";

            var encodedFileName = EncodeString(file);

            string cacheKey = $"{encodedFileName}_details";

            VideoData videoData = null;

            if (_cache.TryGetValue(cacheKey, out string cachedValue))
            {
                videoData = JsonSerializer.Deserialize<VideoData>(cachedValue);
            }
            else
            {
                var videoFilePath = GetFullVideoPath(file);
                videoData = await GetVideoDetails(videoFilePath);
                _cache.Set(cacheKey, JsonSerializer.Serialize(videoData), _cacheEntryOptions);
            }

            var masterPlaylist = "#EXTM3U\n\n" +
                                    "#EXT-X-STREAM-INF:BANDWIDTH=800000,RESOLUTION=360x640\n" +
                                    $"{_apiBaseUrl}/api/streaming/{encodedFileName}/360p.m3u8\n\n" +
                                    "#EXT-X-STREAM-INF:BANDWIDTH=1000000,RESOLUTION=480x854\n" +
                                    $"{_apiBaseUrl}/api/streaming/{encodedFileName}/480p.m3u8\n\n" +
                                    "#EXT-X-STREAM-INF:BANDWIDTH=1400000,RESOLUTION=720x1280\n" +
                                    $"{_apiBaseUrl}/api/streaming/{encodedFileName}/720p.m3u8\n\n" +
                                    "#EXT-X-STREAM-INF:BANDWIDTH=2800000,RESOLUTION=1080x1920\n" +
                                    $"{_apiBaseUrl}/api/streaming/{encodedFileName}/1080p.m3u8\n\n";

            System.IO.File.WriteAllText($"master.m3u8", masterPlaylist);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(masterPlaylist));
            return new FileStreamResult(stream, "application/vnd.apple.mpegurl");
        }

        // Serve dynamic playlist based on video duration and resolution
        [HttpGet("{encodedFileName}/{quality}.m3u8")]
        public async Task<IActionResult> GetQualityPlaylist([FromRoute] string quality, [FromRoute] string encodedFileName)
        {
            string decodedString = DecodeString(encodedFileName);
            var videoFilePath = GetFullVideoPath(decodedString);

            string cacheKey = $"{encodedFileName}_details";
            VideoData videoData = null;

            if (_cache.TryGetValue(cacheKey, out string cachedValue))
            {
                videoData = JsonSerializer.Deserialize<VideoData>(cachedValue);
            }
            else
            {
                videoData = await GetVideoDetails(videoFilePath);
                _cache.Set(cacheKey, JsonSerializer.Serialize(videoData), _cacheEntryOptions);
            }

            double videoDuration = videoData.GetDurationInSeconds();

            int totalSegments = (int)Math.Floor(videoDuration / SegmentDuration);

            double decimalSegmentDuration = videoDuration % SegmentDuration;

            string _apiBaseUrl = $"{Request.Scheme}://{Request.Host}";

            var playlist = "#EXTM3U\n" +
                           "#EXT-X-VERSION:6\n" +
                           $"#EXT-X-TARGETDURATION:{SegmentDuration}\n" +
                           "#EXT-X-PLAYLIST-TYPE:VOD\n" +
                           "#EXT-X-MEDIA-SEQUENCE:0\n" +
                            "#EXT-X-INDEPENDENT-SEGMENTS\n";

            for (int i = 0; i < totalSegments; i++)
            {
                var videoIndex = i.ToString().PadLeft(totalSegments.ToString().Count(), '0');

                playlist += $"#EXTINF:{SegmentDuration}.000000,\n" +
                            $"{_apiBaseUrl}/api/streaming/segment/{encodedFileName}/{quality}_{videoIndex}.ts\n";
            }

            if (decimalSegmentDuration > 0)
            {
                playlist += $"#EXTINF:{Math.Round(decimalSegmentDuration, 6)},\n" +
                            $"{_apiBaseUrl}/api/streaming/segment/{encodedFileName}/{quality}_{totalSegments}.ts\n";
            }

            playlist += "#EXT-X-ENDLIST\n";

            //return Content(playlist, "application/vnd.apple.mpegurl");


            System.IO.File.WriteAllText($"{quality}.m3u8", playlist);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(playlist));
            return new FileStreamResult(stream, "application/vnd.apple.mpegurl");
        }

        // Serve video segments on-demand for any resolution
        [HttpGet("segment/{encodedFileName}/{quality}_{index}.ts")]
        public async Task<IActionResult> GetVideoSegment(string quality, int index, string encodedFileName)
        {
            string decodedString = DecodeString(encodedFileName);
            var filePath = GetFullVideoPath(decodedString);

            string resolution = quality switch
            {
                "360p" => "640:360",
                "480p" => "854:480",
                "720p" => "1280:720",
                "1080p" => "1920:1080",
                _ => "640x360"
            };

            // Cache key for the specific segment
            string cacheKey = $"{resolution}_{index}_{encodedFileName}";

            if (_cache.TryGetValue(cacheKey, out byte[] cachedSegment))
            {
                return File(cachedSegment, "video/mp2t");
            }

            double startTime = index * SegmentDuration;

            // Use `-ss` for precise seeking to a specific segment and align with keyframes
            string segmentCommand = $"-ss {startTime} -i \"{filePath}\" -vf scale={resolution} " +
                                $"-c:v h264 -preset ultrafast " + // Video encoding libx264, h264
                                $"-c:a aac -b:a 128k -ac 2 " + // Audio encoding
                                                               //$"-g {SegmentDuration * 2} " + // Keyframe interval set to match segment duration
                                $" -output_ts_offset {startTime} -threads 4 " +
                                $"-f mpegts -t {SegmentDuration} pipe:1";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = segmentCommand,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process == null)
                {
                    return StatusCode(500, "Error starting FFmpeg process");
                }

                await using var memoryStream = new MemoryStream();
                await process.StandardOutput.BaseStream.CopyToAsync(memoryStream);

                var segmentData = memoryStream.ToArray();

                // Cache the segment for future requests
                _cache.Set(cacheKey, segmentData, _cacheEntryOptions);

                return File(segmentData, "video/mp2t");
                //return File(process.StandardOutput.BaseStream, "video/mp2t");
            }

        }

        private string GetFullVideoPath(string fileName)
        {
            return Path.Combine(videoPath, fileName);
        }

        private string EncodeString(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        private string DecodeString(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        // Extract video duration using FFprob
        private async Task<VideoData> GetVideoDetails(string videoPath)
        {
            VideoData? videoData = null;

            var processStartInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = $"-v error -select_streams v:0 -show_entries stream=width,height -of json \"{videoPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            using (var reader = process.StandardOutput)
            {
                var output = reader.ReadToEnd();
                VideoDetails? videoDetails = JsonSerializer.Deserialize<VideoDetails>(output);

                videoData = videoDetails?.streams.FirstOrDefault();
            }

            //var output = await RunCommandAsync(ffprobePath, $"-v error -select_streams v:0 -show_entries stream=width,height -of json \"{videoPath}\"");
            //if (output != null)
            //{
            //    VideoDetails? videoDetails = JsonSerializer.Deserialize<VideoDetails>(output);

            //    videoData = videoDetails?.streams.FirstOrDefault();

            //}

            processStartInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{videoPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            using (var reader = process.StandardOutput)
            {
                var output = reader.ReadToEnd();

                if (videoData != null)
                {
                    videoData.duration = output;
                }
            }

            //output = await RunCommandAsync(ffprobePath, $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{videoPath}\"");
            //if (output != null)
            //{
            //    videoData.duration = output;
            //}

            processStartInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = $"-v 0 -select_streams v:0 -show_entries stream=r_frame_rate -of default=noprint_wrappers=1:nokey=1 \"{videoPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            using (var reader = process.StandardOutput)
            {
                var output = reader.ReadToEnd();

                if (videoData != null)
                {
                    videoData.fps = output.Split('/')[0];
                }
            }

            //output = await RunCommandAsync(ffprobePath, $"-v 0 -select_streams v:0 -show_entries stream=r_frame_rate -of default=noprint_wrappers=1:nokey=1 \"{videoPath}\"");
            //if (output != null)
            //{
            //    videoData.fps = output.Split('/')[0];
            //}

            return videoData;
        }

        private async Task<string> RunCommandAsync(string command, string args)
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

                return output;

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg error: {error}");
                }
            }
        }


    }
}
