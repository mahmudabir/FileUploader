using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class VideoController : ControllerBase
{
    private readonly string _videoPath = Path.Combine(Directory.GetCurrentDirectory(), "..\\Uploads");
    private readonly int _chunkSize = 10 * 1024 * 1024; // 10 MB

    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetVideoChunk(string fileName, [FromQuery] long start = 0, [FromQuery] bool getFullSize = false)
    {
        var filePath = Path.Combine(_videoPath, fileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var fileInfo = new FileInfo(filePath);
        var fileSize = fileInfo.Length;

        if (start >= fileSize)
            return BadRequest("Invalid start position");

        var end = Math.Min(start + _chunkSize - 1, fileSize - 1);

        if (getFullSize)
        {
            end = fileSize - 1;
        }

        var length = end - start + 1;

        Response.Headers.Add("Accept-Ranges", "bytes");
        Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileSize}");
        Response.Headers.Add("X-Total-File-Size", fileSize.ToString());
        Response.ContentType = GetContentType(filePath);

        Response.Headers.Add("Content-Length", length.ToString());

        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            stream.Seek(start, SeekOrigin.Begin);
            var buffer = new byte[length];
            await stream.ReadAsync(buffer, 0, (int)length);
            return File(buffer, Response.ContentType);
        }
    }

    private string GetContentType(string filePath)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out var contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }
}