using Microsoft.AspNetCore.Mvc;

namespace FileUploader.Models
{
    public class FileUpload
    {
        public IFormFile chunk { get; set; }
        public string fileName { get; set; }
        public long chunkNumber { get; set; }
        public long totalChunks { get; set; }
    }
}
