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
        private const double SegmentDuration = 15; // Segment duration in seconds

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
            var masterPlaylist = await GenerateMasterPlaylist(file);


            System.IO.File.WriteAllText($"master.m3u8", masterPlaylist);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(masterPlaylist));
            return new FileStreamResult(stream, "application/vnd.apple.mpegurl");
        }

        // Serve dynamic playlist based on video duration and resolution
        [HttpGet("{encodedFileName}/{quality}.m3u8")]
        public async Task<IActionResult> GetQualityPlaylist([FromRoute] string quality, [FromRoute] string encodedFileName)
        {
            string playlist = await GenerateSegmentPlaylist(quality, encodedFileName);

            System.IO.File.WriteAllText($"{quality}.m3u8", playlist);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(playlist));
            return new FileStreamResult(stream, "application/vnd.apple.mpegurl");
        }

        // Serve video segments on-demand for any resolution
        [HttpGet("{encodedFileName}/segment/{quality}_{index}.ts")]
        public async Task<IActionResult> GetVideoSegment(string quality, int index, string encodedFileName)
        {
            string decodedString = DecodeString(encodedFileName);
            var videoFilePath = GetFullVideoPath(decodedString);
            string videoDataCacheKey = $"{encodedFileName}_details";
            VideoData videoData = null;

            if (_cache.TryGetValue(videoDataCacheKey, out string cachedValue))
            {
                videoData = JsonSerializer.Deserialize<VideoData>(cachedValue);
            }
            else
            {
                videoData = await GetVideoDetails(videoFilePath);
                _cache.Set(videoDataCacheKey, JsonSerializer.Serialize(videoData), _cacheEntryOptions);
            }

            var threadCount = "4";
            var videoCodec = "libx264"; // Default Video encoding: libx264 (Another optino h264)
            var audioCodec = "aac"; // Default Audio encoding aac
            var defaultFrameRate = "30"; // Default Frame Rate
            var audioChannel = "2"; // Default Audio Channel

            var videoFrameRate = videoData?.fps ?? defaultFrameRate;

            (string Resolution, string VideoBitrate, string VideoCodec, string FrameRate, string AudioBitrate, string AudioCodec, string AudioSampleRate, string AudioChannel, string Preset) config = quality
            switch
            {
                "144p" => ("256:144", "300k", videoCodec, defaultFrameRate, "64k", audioCodec, "44100", audioChannel, "ultrafast"),
                "240p" => ("426:240", "500k", videoCodec, defaultFrameRate, "96k", audioCodec, "44100", audioChannel, "ultrafast"),
                "360p" => ("640:360", "800k", videoCodec, defaultFrameRate, "128k", audioCodec, "48000", audioChannel, "ultrafast"),
                "480p" => ("854:480", "1200k", videoCodec, defaultFrameRate, "128k", audioCodec, "48000", audioChannel, "ultrafast"),
                "720p" => ("1280:720", "2500k", videoCodec, videoFrameRate, "160k", audioCodec, "48000", audioChannel, "ultrafast"),
                "1080p" => ("1920:1080", "4500k", videoCodec, videoFrameRate, "192k", audioCodec, "48000", audioChannel, "ultrafast"),
                "2k" => ("2560:1440", "8000k", videoCodec, videoFrameRate, "192k", audioCodec, "48000", audioChannel, "ultrafast"),
                "4k" => ("3840:2160", "20000k", videoCodec, videoFrameRate, "256k", audioCodec, "48000", audioChannel, "ultrafast"),
                "8k" => ("7680:4320", "40000k", videoCodec, videoFrameRate, "320k", audioCodec, "48000", audioChannel, "ultrafast"),
                _ => ("640:360", "800k", videoCodec, defaultFrameRate, "128k", audioCodec, "48000", audioChannel, "ultrafast"),
            };

            // Cache key for the specific segment
            string cacheKey = $"{config.Resolution}_{config.VideoBitrate}_{index}_{encodedFileName}";

            if (_cache.TryGetValue(cacheKey, out byte[] cachedSegment))
            {
                return File(cachedSegment, "video/mp2t");
            }

            double startTime = index * SegmentDuration;

            string segmentCommand = $"-ss {startTime} -i \"{videoFilePath}\" -vf scale={config.Resolution} -r {config.FrameRate} " +
                                $"-c:v {config.VideoCodec} -b:v {config.VideoBitrate} " +
                                $"-c:a {config.AudioCodec} -b:a {config.AudioBitrate} -ar {config.AudioSampleRate} -ac {config.AudioChannel} " +
                                $"-preset {config.Preset} " +
                                $"-g {SegmentDuration * 2} " + // Keyframe interval set to match segment duration
                                $" -output_ts_offset {startTime} -threads {threadCount} " +
                                $"-f mpegts -t {SegmentDuration} pipe:1";

            System.IO.File.WriteAllText("ffmpeg.txt", "ffmpeg " + segmentCommand);

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

                if (segmentData.Length > 0)
                {
                    // Cache the segment for future requests
                    _cache.Set(cacheKey, segmentData, _cacheEntryOptions);
                }

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
                    var numerator = output.Split('/')[0];
                    var denominator = output.Split('/')[1];
                    videoData.fps = (Convert.ToInt32(numerator) / Convert.ToInt32(denominator)).ToString();
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

        private async Task<string> GenerateMasterPlaylist(string file)
        {
            string _apiBaseUrl = $"{Request.Scheme}://{Request.Host}";
            var playlistPathPrefix = $"{_apiBaseUrl}/api/streaming";

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

            var qualityLevels = new List<(int bandwidth, string resolution, int height, int width)>
            {
                ((300 * 1000), "144p", 144, 256),
                ((500 * 1000), "240p", 240, 426),
                ((800 * 1000), "360p", 360, 640),
                ((1200 * 1000), "480p", 480, 854),
                ((2500 * 1000), "720p", 720, 1280),
                ((4500 * 1000), "1080p", 1080, 1920),
                ((8000 * 1000), "1440p", 1440, 2560),
                ((20000 * 1000), "4k", 2160, 3840),
                ((40000 * 1000), "8k", 4320, 7680)
            };

            var masterPlaylist = "#EXTM3U\n\n";

            foreach (var (bandwidth, resolution, height, width) in qualityLevels)
            {
                // Only include qualities that are smaller than or equal to the current video resolution
                if (height <= videoData.height && width <= videoData.width)
                {
                    masterPlaylist += $"#EXT-X-STREAM-INF:BANDWIDTH={bandwidth},RESOLUTION={height}x{width}\n";
                    masterPlaylist += $"{encodedFileName}/{resolution}.m3u8\n\n";
                }
            }

            return masterPlaylist;
        }

        private async Task<string> GenerateSegmentPlaylist(string quality, string encodedFileName)
        {
            string _apiBaseUrl = $"{Request.Scheme}://{Request.Host}";
            var playlistPathPrefix = $"{_apiBaseUrl}/api/streaming";

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
                            $"segment/{quality}_{videoIndex}.ts\n";
            }

            if (decimalSegmentDuration > 0)
            {
                playlist += $"#EXTINF:{Math.Round(decimalSegmentDuration, 6)},\n" +
                            $"segment/{quality}_{totalSegments}.ts\n";
            }

            playlist += "#EXT-X-ENDLIST\n";
            return playlist;
        }
    }
}
