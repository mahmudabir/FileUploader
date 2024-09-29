namespace FileUploader.Models
{
    public class VideoDetails
    {
        public object[] programs { get; set; }
        public object[] stream_groups { get; set; }
        public VideoData[] streams { get; set; }
    }

    public class VideoData
    {
        public int width { get; set; }
        public int height { get; set; }
        public string duration { get; set; }
        public string fps { get; set; }
    }

}
