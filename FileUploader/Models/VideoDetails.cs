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


    public class VideoKeyFrame
    {
        public List<Frame> frames { get; set; }
    }

    public class Frame
    {
        public long key_frame { get; set; }
        public List<SideDataList> side_data_list { get; set; }
    }

    public class SideDataList
    {
        public string side_data_type { get; set; }
    }
}
