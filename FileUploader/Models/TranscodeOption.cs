﻿namespace FileUploader.Models
{
    public class TranscodeOption
    {
        public bool IsCachingEnabled { get; set; }
        public string FFmpegPath { get; set; }
        public string FFprobePath { get; set; }
        public int SegmentDuration { get; set; }
        public string OutputDirectory { get; set; }
        public string DefaultFrameRate { get; set; }
        public int DefaultThreadCount { get; set; }

        public bool ForceDisableGpuUse { get; set; }
        public bool TranscodeVideo { get; set; }
        public bool DisableVideoBitrateTranscoding { get; set; }
        public bool TranscodeAudio { get; set; }

        public bool IsGpuEnabled { 
            get
            {
                return !ForceDisableGpuUse && GpuType != GpuType.None && DefaultVideoCodec.Contains("_");
            }
        }
        public bool IsMultiThreadingEnabled
        {
            get
            {
                return DefaultThreadCount > 1;
            }
        }

        public GpuType GpuType { get; set; }
        public string DefaultVideoCodec { get; set; }
        public string DefaultAudioCodec { get; set; }
        public string DefaultPreset { get; set; }

        public string CurrenDirectory { get; set; }
        public string UploadDirectory { get; set; }
    }
}
