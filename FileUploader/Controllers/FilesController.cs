using Microsoft.AspNetCore.Mvc;

namespace FileUploader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly string videoPath = Path.Combine(Directory.GetCurrentDirectory(), "..\\Uploads");

        private static readonly string[] allVideoExtensions = [".webm", ".mkv", ".flv", ".flv", ".vob", ".ogv", ".ogg", ".drc", ".gif", ".gifv", ".mng", ".avi", ".MTS", ".M2TS", ".TS", ".mov", ".qt", ".wmv", ".yuv", ".rm", ".rmvb", ".viv", ".asf", ".amv", ".mp4", ".m4p", ".m4v", ".mpg", ".mp2", ".mpeg", ".mpe", ".mpv", ".mpg", ".mpeg", ".m2v", ".m4v", ".svi", ".3gp", ".3g2", ".mxf", ".roq", ".nsv", ".flv", ".f4v", ".f4p", ".f4a", ".f4b"];


        // GET: api/files
        [HttpGet]
        public IActionResult GetFiles([FromQuery] bool videosOnly = true)
        {
            if (!Directory.Exists(videoPath))
            {
                return NotFound("Directory not found.");
            }

            // Get all files from the folder
            var files = Directory.GetFiles(videoPath);

            // If no files, return empty
            if (files.Length == 0)
            {
                return Ok("No files found in the directory.");
            }

            // Return the list of file names
            List<string?> fileNames = files.Select(Path.GetFileName).ToList();

            if (videosOnly)
            {
                fileNames = fileNames.Where(x => allVideoExtensions.Contains(Path.GetExtension(x))).ToList();


            }

            return Ok(fileNames);
        }
    }
}
