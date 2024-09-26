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

//ffmpeg - i "inputPath.mp4" - profile:v baseline -level 3.0 -s 640x360 -start_number 0 -hls_time 10 -hls_list_size 0 -f hls "outputPath.m3u8"

using System.Threading;

//ffmpeg - i input.mkv - codec:v libx264 -codec:a aac -hls_time 10 -hls_playlist_type vod output.m3u8


//ffmpeg -i input.mkv -codec:v libx264 -codec:a aac -hls_time 10 -hls_playlist_type vod -hls_flags independent_segments -threads 8 -preset ultrafast output.m3u8

//ffmpeg -i input.mkv -c:v libx264 -profile:v main -level 3.1 -crf 20 -c:a aac -b:a 128k -ac 2 -hls_time 15 -hls_list_size 0 -hls_flags independent_segments -threads 8 -preset ultrafast output.m3u8


// Multiple quality
//ffmpeg -i inputs.mp4 -filter:v:0 scale=360:640 -c:a aac -ar 48000 -b:a 64k -c:v:0 libx264 -b:v:0 250k -preset ultrafast -g 48 -keyint_min 48 -sc_threshold 0 -hls_time 10 -hls_segment_filename "360p_%03d.ts" -hls_playlist_type vod -hls_flags independent_segments -filter:v:1 scale=720:1280 -c:a aac -ar 48000 -b:a 96k -c:v:1 libx264 -b:v:1 500k -preset ultrafast -g 48 -keyint_min 48 -sc_threshold 0 -hls_time 10 -hls_segment_filename "720p_%03d.ts" -hls_playlist_type vod -hls_flags independent_segments -filter:v:2 scale=1080:1920 -c:a aac -ar 48000 -b:a 128k -c:v:2 libx264 -b:v:2 1500k -preset ultrafast -g 48 -keyint_min 48 -sc_threshold 0 -hls_time 10 -hls_segment_filename "1080p_%03d.ts" -hls_playlist_type vod -hls_flags independent_segments -master_pl_name master.m3u8 -hls_list_size 0 -f hls master.m3u8

//#EXTM3U

//#EXT-X-STREAM-INF:BANDWIDTH=800000,RESOLUTION=360x640
//360p.m3u8

//#EXT-X-STREAM-INF:BANDWIDTH=2800000,RESOLUTION=720x1280
//720p.m3u8

//#EXT-X-STREAM-INF:BANDWIDTH=5000000,RESOLUTION=1080x1920
//1080p.m3u8

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