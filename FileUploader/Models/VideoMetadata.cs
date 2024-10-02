namespace FileUploader.Models
{
    public class VideoMetadata
    {
        public List<VideoStream> streams { get; set; }
        public VideoFormat format { get; set; }
    }

    public class VideoFormat
    {
        public string filename { get; set; }
        public long nb_streams { get; set; }
        public long nb_programs { get; set; }
        public long nb_stream_groups { get; set; }
        public string format_name { get; set; }
        public string format_long_name { get; set; }
        public string start_time { get; set; }
        public string duration { get; set; }
        public long size { get; set; }
        public long bit_rate { get; set; }
        public long probe_score { get; set; }
        public VideoFormatTags tags { get; set; }
    }

    public class VideoFormatTags
    {
        public string major_brand { get; set; }
        public long minor_version { get; set; }
        public string compatible_brands { get; set; }
        public string encoder { get; set; }
    }

    public class VideoStream
    {
        public long index { get; set; }
        public string codec_name { get; set; } //h264, libx264, aac etc.
        public string codec_long_name { get; set; }
        public string profile { get; set; }
        public string codec_type { get; set; } //video, audio, subtitle etc.
        public string codec_tag_string { get; set; }
        public string codec_tag { get; set; }
        public long? width { get; set; }
        public long? height { get; set; }
        public long? coded_width { get; set; }
        public long? coded_height { get; set; }
        public long? closed_captions { get; set; }
        public long? film_grain { get; set; }
        public long? has_b_frames { get; set; }
        public string sample_aspect_ratio { get; set; }
        public string display_aspect_ratio { get; set; }
        public string pix_fmt { get; set; }
        public long? level { get; set; }
        public string color_range { get; set; }
        public string color_space { get; set; }
        public string color_transfer { get; set; }
        public string color_primaries { get; set; }
        public string chroma_location { get; set; }
        public string field_order { get; set; }
        public long? refs { get; set; }
        public bool? is_avc { get; set; }
        public long? nal_length_size { get; set; }
        public string id { get; set; }
        public string r_frame_rate { get; set; }
        public string avg_frame_rate { get; set; }
        public string time_base { get; set; }
        public long start_pts { get; set; }
        public string start_time { get; set; }
        public long duration_ts { get; set; }
        public string duration { get; set; }
        public string? bit_rate { get; set; }
        public long? bits_per_raw_sample { get; set; }
        public long nb_frames { get; set; }
        public long extradata_size { get; set; }
        public Dictionary<string, long> disposition { get; set; }
        public VideoStreamTags tags { get; set; }
        public string sample_fmt { get; set; }
        public string? sample_rate { get; set; }
        public long? channels { get; set; }
        public string channel_layout { get; set; }
        public long? bits_per_sample { get; set; }
        public long? initial_padding { get; set; }
    }

    public class VideoStreamTags
    {
        public string language { get; set; }
        public string handler_name { get; set; }
        public string vendor_id { get; set; }
    }

}
