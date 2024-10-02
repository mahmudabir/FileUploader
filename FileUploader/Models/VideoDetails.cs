namespace FileUploader.Models
{
    public class VideoDetails
    {
        public List<object> programs { get; set; }
        public List<object> stream_groups { get; set; }
        public List<VideoStreamData> streams { get; set; }
        public VideoStreamDuration format { get; set; }
    }

    public class VideoStreamDuration
    {
        public string duration { get; set; }
    }

    public class VideoStreamData
    {
        public string? codec_type { get; set; } //video, audio, subtitle
        public string? codec_name { get; set; } //h264, libx264, aac
        public string? sample_rate { get; set; } //h264, libx264, aac
        public int? channels { get; set; } //h264, libx264, aac
        public long? width { get; set; }
        public long? height { get; set; }
        public string? r_frame_rate { get; set; }
        public string? bit_rate { get; set; }
    }

    public class VideoData
    {
        public long? Width { get; set; }
        public long? Height { get; set; }

        public double Duration { get; set; }

        public string? FrameRate { get; set; }
        public string? VideoBitRate { get; set; }
        public string? VideoCodec { get; set; }

        public string? AudioBitRate { get; set; }
        public string? AudioCodec { get; set; }
        public string? AudioSampleRate { get; set; }
        public int? AudioChannels { get; set; }
    }

    public static class VideoExtensions
    {
        public static VideoData ToVideoData(this VideoDetails videoStreamData)
        {
            var videoDetailsData = videoStreamData.streams.FirstOrDefault(x => x.codec_type == "video");
            var audioDetailsData = videoStreamData.streams.FirstOrDefault(x => x.codec_type == "audio");

            var frameRateStr = videoStreamData.streams.FirstOrDefault(x => x.codec_type == "video")?.r_frame_rate;
            var numerator = frameRateStr?.Split('/')[0];
            var denominator = frameRateStr?.Split('/')[1];
            var fps = Convert.ToDouble(numerator) / Convert.ToDouble(denominator);
            fps = Math.Round((fps == 0 ? 30 : fps), 2);

            VideoData videoData = new VideoData
            {
                Width = videoDetailsData?.width,
                Height = videoDetailsData?.height,

                Duration = Convert.ToDouble(videoStreamData.format.duration),

                FrameRate = fps.ToString(),
                VideoBitRate = videoDetailsData?.bit_rate,
                VideoCodec = videoDetailsData?.codec_name,

                AudioBitRate = audioDetailsData?.bit_rate,
                AudioCodec = audioDetailsData?.codec_name,
                AudioSampleRate = audioDetailsData?.sample_rate,
                AudioChannels = audioDetailsData?.channels,
            };
            return videoData;
        }

        public static VideoStreamingConfiguration ToVideoStreamingConfiguration(this VideoData videoData)
        {
            var defaultPreset = "ultrafast";
            return new VideoStreamingConfiguration(
                $"{videoData.Width}:{videoData.Height}",
                videoData.Duration,
                videoData.VideoBitRate,
                videoData.VideoCodec,
                videoData.FrameRate,
                videoData.AudioBitRate,
                videoData.AudioCodec,
                videoData.AudioSampleRate,
                videoData.AudioChannels,
                defaultPreset
                );
        }
    }


    public record VideoStreamingConfiguration(
        string Resolution,
        double Duration,
        string VideoBitRate,
        string VideoCodec,
        string FrameRate,
        string AudioBitRate,
        string AudioCodec,
        string AudioSampleRate,
        int? AudioChannels,
        string Preset,
        string? ResolutionName = "Default"
        );
}
