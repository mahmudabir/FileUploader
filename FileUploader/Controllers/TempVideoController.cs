//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.StaticFiles;
//using System.IO;

//[ApiController]
//[Route("api/[controller]")]
//public class VideoController : ControllerBase
//{
//    private readonly string _videoPath = Path.Combine(Directory.GetCurrentDirectory(), "..\\Uploads");
//    private readonly int _chunkSize = 10 * 1024 * 1024; // 10 MB

//    [HttpGet("{fileName}")]
//    public async Task<IActionResult> GetVideoChunk(string fileName, [FromQuery] long start = 0)
//    {
//        var filePath = Path.Combine(_videoPath, fileName);

//        if (!System.IO.File.Exists(filePath))
//            return NotFound();

//        var fileInfo = new FileInfo(filePath);
//        var fileSize = fileInfo.Length;

//        if (start >= fileSize)
//            return BadRequest("Invalid start position");

//        var end = Math.Min(start + _chunkSize - 1, fileSize - 1);
//        //end = fileSize - 1;
//        var length = end - start + 1;

//        Response.Headers.Add("Accept-Ranges", "bytes");
//        Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileSize}");
//        Response.ContentType = GetContentType(filePath);

//        Response.Headers.Add("Content-Length", length.ToString());

//        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
//        {
//            stream.Seek(start, SeekOrigin.Begin);
//            var buffer = new byte[length];
//            await stream.ReadAsync(buffer, 0, (int)length);
//            return File(buffer, Response.ContentType);
//        }
//    }

//    private string GetContentType(string filePath)
//    {
//        var provider = new FileExtensionContentTypeProvider();
//        if (!provider.TryGetContentType(filePath, out var contentType))
//        {
//            contentType = "application/octet-stream";
//        }
//        return contentType;
//    }
//}

//=======================


//<!DOCTYPE html>
//<html lang="en">

//<head>
//    <meta charset="UTF-8">
//    <meta name="viewport" content="width=device-width, initial-scale=1.0">
//    <title>Document</title>
//</head>

//<body>

//    <script src="https://cdn.fluidplayer.com/v3/current/fluidplayer.min.js"></script>
//    <!-- height="360" width="640" -->
//     <!-- Avatar The Last Airbender S01E01.mp4 -->
//    <!-- Wednesday S01E02  1080p NF WEBRip x265 HEVC MSubs [Dual Audio][Hindi 5.1+English 5.1] -OlaM.mkv -->
//    <div style="height: 360px; width: 640px;">
//        <video id="video-id">
//            <source
//                src="https://localhost:7001/api/filedownload/download/Avatar The Last Airbender S01E01.mp4"
//                type="video/mp4" />
//        </video>
//    </div>

//    <script>
//        var myFP = fluidPlayer(
//            'video-id', {
//            "layoutControls": {
//                "controlBar": {
//                    "autoHideTimeout": 3,
//                    "animated": true,
//                    "autoHide": true
//                },
//                "htmlOnPauseBlock": {
//                    "html": null,
//                    "height": null,
//                    "width": null
//                },
//                "autoPlay": false,
//                "mute": true,
//                "allowTheatre": true,
//                "playPauseAnimation": false,
//                "playbackRateEnabled": false,
//                "allowDownload": false,
//                "playButtonShowing": false,
//                "fillToContainer": true,
//                "posterImage": ""
//            },
//            "vastOptions": {
//                "adList": [],
//                "adCTAText": false,
//                "adCTATextPosition": ""
//            }
//        });
//    </script>
//</body>

//</html><!DOCTYPE html>
//<html lang="en">

//<head>
//    <meta charset="UTF-8">
//    <meta name="viewport" content="width=device-width, initial-scale=1.0">
//    <title>Document</title>
//</head>

//<body>

//    <script src="https://cdn.fluidplayer.com/v3/current/fluidplayer.min.js"></script>
//    <!-- height="360" width="640" -->
//     <!-- Avatar The Last Airbender S01E01.mp4 -->
//    <!-- Wednesday S01E02  1080p NF WEBRip x265 HEVC MSubs [Dual Audio][Hindi 5.1+English 5.1] -OlaM.mkv -->
//    <div style="height: 360px; width: 640px;">
//        <video id="video-id">
//            <source
//                src="https://localhost:7001/api/filedownload/download/Avatar The Last Airbender S01E01.mp4"
//                type="video/mp4" />
//        </video>
//    </div>

//    <script>
//        var myFP = fluidPlayer(
//            'video-id', {
//            "layoutControls": {
//                "controlBar": {
//                    "autoHideTimeout": 3,
//                    "animated": true,
//                    "autoHide": true
//                },
//                "htmlOnPauseBlock": {
//                    "html": null,
//                    "height": null,
//                    "width": null
//                },
//                "autoPlay": false,
//                "mute": true,
//                "allowTheatre": true,
//                "playPauseAnimation": false,
//                "playbackRateEnabled": false,
//                "allowDownload": false,
//                "playButtonShowing": false,
//                "fillToContainer": true,
//                "posterImage": ""
//            },
//            "vastOptions": {
//                "adList": [],
//                "adCTAText": false,
//                "adCTATextPosition": ""
//            }
//        });
//    </script>
//</body>

//</html>