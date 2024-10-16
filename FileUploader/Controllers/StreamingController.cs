﻿using FileUploader.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace FileUploader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamingController : ControllerBase
    {
        private readonly string _uploadDirectory;

        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions;
        private readonly TranscodeOption? _transcodeOption;

        public StreamingController(IMemoryCache cache, IOptionsMonitor<TranscodeOption> optionsMonitor)
        {
            _cache = cache;
            _cacheEntryOptions = new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };

            _transcodeOption = optionsMonitor.CurrentValue;
            _uploadDirectory = _transcodeOption.UploadDirectory;
        }

        [HttpPost("clear-cache")]
        public IActionResult ClearAllCache()
        {
            if (_cache is MemoryCache cache)
            {
                cache.Clear();
            }

            Console.Clear();
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
        [HttpGet("{encodedFileName}/{quality}_{height}_{width}.m3u8")]
        public async Task<IActionResult> GetQualityPlaylist([FromRoute] string quality, [FromRoute] int height, [FromRoute] int width, [FromRoute] string encodedFileName)
        {
            string playlist = await GenerateSegmentPlaylist(quality, height, width, encodedFileName);

            System.IO.File.WriteAllText($"{quality}.m3u8", playlist);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(playlist));
            return new FileStreamResult(stream, "application/vnd.apple.mpegurl");
        }

        // Serve video segments on-demand for any resolution
        [HttpGet("{encodedFileName}/segment/{quality}_{height}_{width}_{index}.ts")]
        public async Task<IActionResult> GetVideoSegment(string quality, int index, int height, int width, string encodedFileName)
        {
            string decodedString = DecodeString(encodedFileName);
            string videoFilePath = GetFullVideoPath(decodedString);
            VideoData videoData = await GetVideoDetails(decodedString);

            var defaultPreset = _transcodeOption.DefaultPreset; // ultrafast, superfast, veryfast, faster, fast, medium, slow, slower, veryslow
            var defaultThreadCount = _transcodeOption.DefaultThreadCount;
            var useMultiThread = _transcodeOption.IsMultiThreadingEnabled;

            var defaultVideoCodec = _transcodeOption.DefaultVideoCodec;// Default Video encoding: h264, hevc, libx264
            var defaultAudioCodec = _transcodeOption.DefaultAudioCodec;

            var useGpu = _transcodeOption.IsGpuEnabled;

            var defaultFrameRate = _transcodeOption.DefaultFrameRate;
            var videoFrameRate = videoData?.FrameRate ?? defaultFrameRate;

            var isFpsMoreThan30 = Convert.ToDouble(videoFrameRate) > 30;

            VideoStreamingConfiguration defaultConfig = videoData.ToVideoStreamingConfiguration();
            var transcodeVideo = _transcodeOption.TranscodeVideo;
            var disableVideoBitrateTranscoding = _transcodeOption.DisableVideoBitrateTranscoding;
            var transcodeAudio = defaultAudioCodec != defaultConfig.AudioCodec && _transcodeOption.TranscodeAudio;

            //// Standard by ChatGPT
            //VideoStreamingConfiguration config = quality switch
            //{
            //    "144p" => new VideoStreamingConfiguration("256:144", 0, "300k", defaultVideoCodec, defaultFrameRate, "64k", defaultAudioCodec, "44100", defaultConfig.AudioChannels, defaultPreset),
            //    "240p" => new VideoStreamingConfiguration("426:240", 0, "500k", defaultVideoCodec, defaultFrameRate, "96k", defaultAudioCodec, "44100", defaultConfig.AudioChannels, defaultPreset),
            //    "360p" => new VideoStreamingConfiguration("640:360", 0, "800k", defaultVideoCodec, defaultFrameRate, "128k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    "480p" => new VideoStreamingConfiguration("854:480", 0, "1200k", defaultVideoCodec, defaultFrameRate, "128k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    "720p" => new VideoStreamingConfiguration("1280:720", 0, "2500k", defaultVideoCodec, videoFrameRate, "160k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    "1080p" => new VideoStreamingConfiguration("1920:1080", 0, "4500k", defaultVideoCodec, videoFrameRate, "192k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    "2k" => new VideoStreamingConfiguration("2560:1440", 0, "8000k", defaultVideoCodec, videoFrameRate, "192k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    "4k" => new VideoStreamingConfiguration("3840:2160", 0, "20000k", defaultVideoCodec, videoFrameRate, "256k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    "8k" => new VideoStreamingConfiguration("7680:4320", 0, "40000k", defaultVideoCodec, videoFrameRate, "320k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    _ => defaultConfig,
            //};

            // Resolution video wise
            VideoStreamingConfiguration config = quality switch
            {
                "144p" => new VideoStreamingConfiguration($"{width}:{height}", 0, "300k", defaultVideoCodec, defaultFrameRate, "64k", defaultAudioCodec, "44100", defaultConfig.AudioChannels, defaultPreset),
                "240p" => new VideoStreamingConfiguration($"{width}:{height}", 0, "500k", defaultVideoCodec, defaultFrameRate, "96k", defaultAudioCodec, "44100", defaultConfig.AudioChannels, defaultPreset),
                "360p" => new VideoStreamingConfiguration($"{width}:{height}", 0, "800k", defaultVideoCodec, defaultFrameRate, "128k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
                "480p" => new VideoStreamingConfiguration($"{width}:{height}", 0, "1200k", defaultVideoCodec, defaultFrameRate, "128k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
                "720p" => new VideoStreamingConfiguration($"{width}:{height}", 0, "2500k", defaultVideoCodec, videoFrameRate, "160k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
                "1080p" => new VideoStreamingConfiguration($"{width}:{height}", 0, "4500k", defaultVideoCodec, videoFrameRate, "192k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
                "2k" => new VideoStreamingConfiguration($"{width}:{height}", 0, "8000k", defaultVideoCodec, videoFrameRate, "192k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
                "4k" => new VideoStreamingConfiguration($"{width}:{height}", 0, "20000k", defaultVideoCodec, videoFrameRate, "256k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
                "8k" => new VideoStreamingConfiguration($"{width}:{height}", 0, "40000k", defaultVideoCodec, videoFrameRate, "320k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
                _ => defaultConfig,
            };

            //YouTube Recommendation: https://support.google.com/youtube/answer/1722171?hl=en#zippy=%2Cvideo-codec-h%2Caudio-codec-aac-lc%2Ccontainer-mp%2Cframe-rate%2Cbitrate

            //VideoStreamingConfiguration config = quality switch
            //{
            //    "144p" => new VideoStreamingConfiguration("256:144", defaultConfig.Duration, isFpsMoreThan30 ? "450K" : "450k", defaultVideoCodec, defaultFrameRate, "64k", defaultAudioCodec, "44100", defaultConfig.AudioChannels, defaultPreset),
            //    "240p" => new VideoStreamingConfiguration("426:240", defaultConfig.Duration, isFpsMoreThan30 ? "900K" : "900k", defaultVideoCodec, defaultFrameRate, "96k", defaultAudioCodec, "44100", defaultConfig.AudioChannels, defaultPreset),
            //    "360p" => new VideoStreamingConfiguration("640:360", defaultConfig.Duration, isFpsMoreThan30 ? "4M" : "2M", defaultVideoCodec, defaultFrameRate, "128k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    "480p" => new VideoStreamingConfiguration("854:480", defaultConfig.Duration, isFpsMoreThan30 ? "8M" : "4M", defaultVideoCodec, defaultFrameRate, "128k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    "720p" => new VideoStreamingConfiguration("1280:720", defaultConfig.Duration, isFpsMoreThan30 ? "16M" : "8M", defaultVideoCodec, videoFrameRate, "160k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    "1080p" => new VideoStreamingConfiguration("1920:1080", defaultConfig.Duration, isFpsMoreThan30 ? "32M" : "16M", defaultVideoCodec, videoFrameRate, "192k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    "2k" => new VideoStreamingConfiguration("2560:1440", defaultConfig.Duration, isFpsMoreThan30 ? "64M" : "32M", defaultVideoCodec, videoFrameRate, "192k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    "4k" => new VideoStreamingConfiguration("3840:2160", defaultConfig.Duration, isFpsMoreThan30 ? "128M" : "64M", defaultVideoCodec, videoFrameRate, "256k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    "8k" => new VideoStreamingConfiguration("7680:4320", defaultConfig.Duration, isFpsMoreThan30 ? "256M" : "128M", defaultVideoCodec, videoFrameRate, "320k", defaultAudioCodec, "48000", defaultConfig.AudioChannels, defaultPreset),
            //    _ => defaultConfig,
            //};

            // Cache key for the specific segment
            string cacheKey = $"{quality}_{height}_{width}_{index}_{config.VideoBitRate}_{encodedFileName}";

            Stopwatch sw = Stopwatch.StartNew();
            if (_transcodeOption.IsCachingEnabled && _cache.TryGetValue(cacheKey, out byte[] cachedSegment))
            {
                sw.Stop();
                Console.WriteLine($"{quality}_{index}.ts => {sw.Elapsed}");
                return File(cachedSegment, "video/mp2t");
            }

            double segmentDuration = _transcodeOption.SegmentDuration;
            double startTime = index * segmentDuration;

            string segmentCommand = $"-i \"{videoFilePath}\" " +
                                    $"-ss {startTime} " + // From Duration
                                    $"-t {segmentDuration} " + // Till Duration
                                    $@"-vf scale=""{config.Resolution},format=yuv420p"" -r {config.FrameRate} " + // ,format=yuv420p for 10-bit to 8-bit
                                    (transcodeVideo ? $"-c:v {config.VideoCodec} " : "-c:v copy") +
                                    (disableVideoBitrateTranscoding ? "" : $"-b:v {config.VideoBitRate} ") +
                                    $"-c:a {config.AudioCodec} " +
                                    (!transcodeAudio ? $"" : $" -b:a {config.AudioBitRate} -ar {config.AudioSampleRate} -ac {config.AudioChannels} ") +
                                    (useGpu ? "" : $"-preset {config.Preset} ") + //$"-g {SegmentDuration * 2} " + // Frame Skip to match with audio
                                    (useMultiThread ? $"-threads {defaultThreadCount} " : "") +
                                    $"-output_ts_offset {startTime} " + // Used to shift the timestamps of the output by a specified amount of time.
                                    $"-f mpegts " +
                                    $"pipe:1";

            System.IO.File.WriteAllText("ffmpeg.txt", "ffmpeg " + segmentCommand);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = _transcodeOption.FFmpegPath,
                Arguments = segmentCommand,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                sw = Stopwatch.StartNew();
                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        return StatusCode(500, "Error starting FFmpeg process");
                    }

                    sw = Stopwatch.StartNew();
                    if (_transcodeOption.IsCachingEnabled)
                    {
                        await using var memoryStream = new MemoryStream();

                        var bufferSize = 1024 * 64;

                        await process.StandardOutput.BaseStream.CopyToAsync(memoryStream, bufferSize);

                        var segmentData = memoryStream.ToArray();

                        if (segmentData.Length > 0)
                        {
                            _cache.Set(cacheKey, segmentData, _cacheEntryOptions);
                        }

                        sw.Stop();
                        Console.WriteLine($"{quality}_{index}.ts => {sw.Elapsed}");

                        return File(segmentData, "video/mp2t");
                    }
                    else
                    {
                        sw.Stop();
                        Console.WriteLine($"{quality}_{index}.ts => {sw.Elapsed}");

                        return File(process.StandardOutput.BaseStream, "video/mp2t");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }


        }

        private string GetFullVideoPath(string fileName)
        {
            return Path.Combine(_uploadDirectory, fileName);
        }

        private string EncodeString(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        private string DecodeString(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        // Extract video details using FFprob
        private async Task<VideoData> GetVideoDetails(string fileName)
        {
            var encodedFileName = EncodeString(fileName);
            string fullVideoFilePath = GetFullVideoPath(fileName);

            string videoDataCacheKey = $"{encodedFileName}_details";
            VideoData? videoData = null;

            if (_cache.TryGetValue(videoDataCacheKey, out string? cachedValue))
            {
                videoData = JsonSerializer.Deserialize<VideoData>(cachedValue ?? "");
            }
            else
            {
                //var detailsCommandToGetAllDetails = $"-v error -show_format -show_streams -of json  \"{fullVideoFilePath}\""; // Shows every metadata of the file

                var detailsCommand = $"-v error -show_entries format=duration -show_entries stream=codec_type,codec_name,sample_rate,channels,bit_rate,width,height,r_frame_rate -of json \"{fullVideoFilePath}\"";

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _transcodeOption?.FFprobePath,
                    Arguments = detailsCommand,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                System.IO.File.WriteAllText("ffprobe.txt", "ffprobe " + detailsCommand);

                using (var process = Process.Start(processStartInfo))
                using (var reader = process.StandardOutput)
                {
                    var output = await reader.ReadToEndAsync();
                    var videoDetails = JsonSerializer.Deserialize<VideoMetadata>(output);

                    videoData = videoDetails?.ToVideoData();

                    //// Optionally read the output (stdout and stderr)
                    //string output = await process.StandardOutput.ReadToEndAsync();
                    //string error = await process.StandardError.ReadToEndAsync();
                    //await process.WaitForExitAsync();
                    //return output;
                }

                _cache.Set(videoDataCacheKey, JsonSerializer.Serialize(videoData), _cacheEntryOptions);
            }

            return videoData;
        }

        [Obsolete(error: true, message: "Not used anymore")]
        private async Task<string> GenerateMasterPlaylistOld(string file)
        {
            string _apiBaseUrl = $"{Request.Scheme}://{Request.Host}";
            var playlistPathPrefix = $"{_apiBaseUrl}/api/streaming";

            VideoData videoData = await GetVideoDetails(file);
            var encodedFileName = EncodeString(file);

            var qualityLevels = new List<(long bandwidth, string quality, int height, int width)>
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

            foreach (var (bandwidth, quality, height, width) in qualityLevels)
            {
                // Only include qualities that are smaller than or equal to the current video resolution
                if (height <= videoData.Height && width <= videoData.Width) // Work on different aspect ratio to show every type of resolution correctly
                {
                    //masterPlaylist += $"#EXT-X-STREAM-INF:BANDWIDTH={bandwidth},RESOLUTION={height}x{width}\n";
                    //masterPlaylist += $"{encodedFileName}/{quality}.m3u8\n\n";
                    masterPlaylist += GenerateM3U8File(encodedFileName, quality, height, width, bandwidth);
                }
            }

            return masterPlaylist;
        }


        private string GenerateM3U8File(string encodedFileName, string quality, long height, long width, long bandwidth)
        {
            string item = string.Empty;
            item += $"#EXT-X-STREAM-INF:BANDWIDTH={bandwidth},RESOLUTION={height}x{width}\n";
            item += $"{encodedFileName}/{quality}_{height}_{width}.m3u8\n\n";
            return item;
        }

        private async Task<string> GenerateMasterPlaylist(string file)
        {
            string _apiBaseUrl = $"{Request.Scheme}://{Request.Host}";
            var playlistPathPrefix = $"{_apiBaseUrl}/api/streaming";

            var masterPlaylist = "#EXTM3U\n\n";

            VideoData videoData = await GetVideoDetails(file);
            var encodedFileName = EncodeString(file);

            long originalWidth = videoData.Width ?? 0;
            long originalHeight = videoData.Height ?? 0;


            var originalQuality = string.Empty;
            long originalBandwidth = 0;

            // Target heights (144p, 240p, 360p, 480p, 720p, 1080p, 1440p, 2160p, 4320p)
            int[] targetHeights = { 144, 240, 360, 480, 720, 1080, 1440, 2160, 4320 };

            var qualityLevels = new List<(long bandwidth, string quality, int height, int width)>
            {
                ((300 * 1000), "144p", 144, 256),
                ((500 * 1000), "240p", 240, 426),
                ((800 * 1000), "360p", 360, 640),
                ((1200 * 1000), "480p", 480, 854),
                ((2500 * 1000), "720p", 720, 1280),
                ((4500 * 1000), "1080p", 1080, 1920),
                ((8000 * 1000), "1440p", 1440, 2560),
                ((20000 * 1000), "4k", 2160, 3840),
                ((40000 * 1000), "8k", 432, 7680)
            };

            // Compute aspect ratio
            double aspectRatio = (double)originalWidth / (double)originalHeight;

            bool originalResolutionProcessed = false;

            // Loop through target resolutions and generate M3U8 if within bounds
            foreach (var targetHeight in qualityLevels)
            {
                if (targetHeight.height > originalHeight) break;  // Stop if the target is larger than the original

                // Calculate target width based on aspect ratio
                int targetWidth = (int)(targetHeight.height * aspectRatio);

                // Generate the FFmpeg command for the resolution
                masterPlaylist += GenerateM3U8File(encodedFileName, targetHeight.quality, targetHeight.height, targetWidth, targetHeight.bandwidth);

                // Check if we've hit the original resolution
                if (targetHeight.height == originalHeight && targetWidth == originalWidth)
                {
                    originalResolutionProcessed = true;
                    //masterPlaylist += GenerateM3U8File(encodedFileName, targetHeight.quality, targetHeight.height, targetWidth, targetHeight.bandwidth);
                }
            }

            // If the original resolution wasn't processed, generate it explicitly
            if (!originalResolutionProcessed)
            {

                var targetQuality = qualityLevels.Find(x => x.height >= originalHeight && x.width >= originalWidth);
                masterPlaylist += GenerateM3U8File(encodedFileName, targetQuality.quality, originalHeight, originalWidth, targetQuality.bandwidth);
                //GenerateM3U8File(originalWidth, originalHeight, "input.mp4", $"output_{originalHeight}p_original.m3u8");
            }

            //var qualityLevels = new List<(long bandwidth, string quality, int height, int width)>
            //{
            //    ((300 * 1000), "144p", 144, 256),
            //    ((500 * 1000), "240p", 240, 426),
            //    ((800 * 1000), "360p", 360, 640),
            //    ((1200 * 1000), "480p", 480, 854),
            //    ((2500 * 1000), "720p", 720, 1280),
            //    ((4500 * 1000), "1080p", 1080, 1920),
            //    ((8000 * 1000), "1440p", 1440, 2560),
            //    ((20000 * 1000), "4k", 2160, 3840),
            //    ((40000 * 1000), "8k", 4320, 7680)
            //};

            //foreach (var (bandwidth, quality, height, width) in qualityLevels)
            //{
            //    // Only include qualities that are smaller than or equal to the current video resolution
            //    if (height <= videoData.Height && width <= videoData.Width) // Work on different aspect ratio to show every type of resolution correctly
            //    {
            //        masterPlaylist += GenerateM3U8File(encodedFileName, quality, height, width, bandwidth);
            //    }
            //}

            return masterPlaylist;
        }


        private async Task<string> GenerateSegmentPlaylist(string quality, int height, int width, string encodedFileName)
        {
            string _apiBaseUrl = $"{Request.Scheme}://{Request.Host}";
            var playlistPathPrefix = $"{_apiBaseUrl}/api/streaming";

            string decodedString = DecodeString(encodedFileName);
            VideoData videoData = await GetVideoDetails(decodedString);

            double videoDuration = videoData.Duration;
            int totalSegmentsRounded = (int)Math.Floor(videoDuration / _transcodeOption.SegmentDuration);
            double segmentDurationMod = videoDuration % _transcodeOption.SegmentDuration;

            var playlist = "#EXTM3U\n" +
                           "#EXT-X-VERSION:6\n" +
                           $"#EXT-X-TARGETDURATION:{_transcodeOption.SegmentDuration}\n" +
                           "#EXT-X-PLAYLIST-TYPE:VOD\n" +
                           "#EXT-X-MEDIA-SEQUENCE:0\n" +
                           "#EXT-X-INDEPENDENT-SEGMENTS\n";

            for (int i = 0; i < totalSegmentsRounded; i++)
            {
                var videoIndex = i.ToString().PadLeft(totalSegmentsRounded.ToString().Count(), '0');

                playlist += $"#EXTINF:{_transcodeOption.SegmentDuration}.000000,\n" +
                            $"segment/{quality}_{height}_{width}_{videoIndex}.ts\n";
            }

            if (segmentDurationMod > 0)
            {
                playlist += $"#EXTINF:{Math.Round(segmentDurationMod, 6)},\n" +
                            $"segment/{quality}_{height}_{width}_{totalSegmentsRounded}.ts\n";
            }

            playlist += "#EXT-X-ENDLIST\n";
            return playlist;
        }
    }
}
