namespace FileUploader.Models
{
    // More properties will be added later if needed
    public record VideoStreamingConfiguration(
        string Resolution,
        double Duration,
        string VideoBitRate,
        string VideoCodec,
        string FrameRate,
        string AudioBitRate,
        string AudioCodec,
        string AudioSampleRate,
        long? AudioChannels,
        string Preset,
        string? ResolutionName = "Default"
        );

    public class VideoData
    {
        // More properties will be added later if needed

        public long? Width { get; set; }
        public long? Height { get; set; }

        public double Duration { get; set; }

        public string? FrameRate { get; set; }
        public string? VideoBitRate { get; set; }
        public string? VideoCodec { get; set; }

        public string? AudioBitRate { get; set; }
        public string? AudioCodec { get; set; }
        public string? AudioSampleRate { get; set; }
        public long? AudioChannels { get; set; }
    }

    public static class VideoExtensions
    {
        public static VideoData ToVideoData(this VideoMetadata videoStreamData)
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


}
