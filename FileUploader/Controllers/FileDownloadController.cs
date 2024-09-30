using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class FileDownloadController : ControllerBase
{
    private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "..\\Uploads");

    // Download endpoint for streaming file to the client
    [HttpGet("download/{fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        // Get the full path to the file
        string filePath = Path.Combine(_uploadFolder, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new { message = "File not found." });
        }

        // Stream the file content to the client
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        // Set the appropriate content type based on the file extension
        string contentType = GetContentType(filePath);

        // Return a streamed file to the client
        return File(stream, contentType, fileName, enableRangeProcessing: true);
    }

    // Optional: This method can help determine the correct MIME type based on the file extension.
    private string GetContentType(string path)
    {
        var types = new Dictionary<string, string>
        {
            { ".txt", "text/plain" },
            { ".pdf", "application/pdf" },
            { ".zip", "application/zip" },
            { ".jpg", "image/jpeg" },
            { ".png", "image/png" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" }
            // Add more types as needed
        };

        var ext = Path.GetExtension(path).ToLowerInvariant();
        return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
    }
}
