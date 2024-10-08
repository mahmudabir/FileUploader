﻿using FileUploader.Models;

using Microsoft.AspNetCore.Mvc;

namespace FileUploader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "..\\Uploads");

        public FileUploadController()
        {
            if (!Directory.Exists(_uploadFolder))
                Directory.CreateDirectory(_uploadFolder);
        }

        [HttpPost("upload-full")]
        public async Task<IActionResult> Upload([FromForm] FileUpload uploadedFile)
        {
            if (!Directory.Exists(_uploadFolder))
                Directory.CreateDirectory(_uploadFolder);

            string filePath = Path.Combine(_uploadFolder, uploadedFile.fileName);

            try
            {
                // Append chunk to file
                using (var stream = new FileStream(filePath, uploadedFile.chunkNumber == 1 ? FileMode.Create : FileMode.Append, FileAccess.Write))
                {
                    await uploadedFile.chunk.CopyToAsync(stream);
                }

                Console.WriteLine("totalChunks: " + uploadedFile.totalChunks);
                Console.WriteLine("chunkNumber: " + uploadedFile.chunkNumber);
                Console.WriteLine("chunkSize: " + (((uploadedFile.chunk.Length / 1024) / 1024)) + " MB");

                // Check if all chunks have been received
                if (uploadedFile.chunkNumber == uploadedFile.totalChunks)
                {
                    Console.Clear();
                    return Ok(new { message = "File upload completed successfully." });
                }

                return Ok(new { message = "Chunk uploaded successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost("upload-chunk")]
        public async Task<IActionResult> UploadChunk([FromForm] FileUpload uploadedFile)
        {
            if (!Directory.Exists(_uploadFolder))
                Directory.CreateDirectory(_uploadFolder);

            string fileExtension = Path.GetExtension(uploadedFile.fileName);
            uploadedFile.fileName = uploadedFile.fileName.Trim(fileExtension.ToCharArray());

            // Determine the length of the string based on the total number of chunks
            int totalLength = uploadedFile.totalChunks.ToString().Length;

            // Generate the zero-padded chunk string
            string paddedChunkNumber = uploadedFile.chunkNumber.ToString().PadLeft(totalLength, '0');


            string filePath = Path.Combine(_uploadFolder, $"{uploadedFile.fileName}");
            string tempFilePath = filePath;
            string fileDirectoryName = Path.Combine(Path.GetDirectoryName(filePath) ?? "", tempFilePath);
            if (!Directory.Exists(fileDirectoryName))
                Directory.CreateDirectory(fileDirectoryName);

            filePath = Path.Combine(fileDirectoryName, $"{uploadedFile.fileName}~{paddedChunkNumber}{fileExtension}");

            try
            {

                // Append chunk to file
                using (var stream = new FileStream(filePath, uploadedFile.chunkNumber == 1 ? FileMode.Create : FileMode.Append, FileAccess.Write))
                {
                    await uploadedFile.chunk.CopyToAsync(stream);
                }

                Console.WriteLine("chunkNumber: " + paddedChunkNumber);

                // Check if all chunks have been received
                if (uploadedFile.chunkNumber == uploadedFile.totalChunks)
                {
                    string combinedFilePath = Path.Combine(_uploadFolder, $"{uploadedFile.fileName}{fileExtension}");

                    Task.Run(async () =>
                    {
                        await CombineFilesAsync(filePath, combinedFilePath);
                    });

                    //await CombineFilesAsync(filePath, combinedFilePath);

                    Console.Clear();
                    return Ok(new { message = "File upload completed successfully." });
                }

                return Ok(new { message = "Chunk uploaded successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        private async Task CombineFilesAsync(string filePath, string outputFilePath)
        {
            // Get all the files in the directory
            string[] files = Directory.GetFiles(Path.GetDirectoryName(filePath));
            files = files.OrderBy(x => x).ToArray();

            // Create the output file stream
            using (FileStream outputStream = System.IO.File.Create(outputFilePath))
            {
                // Combine the contents of all the files into the output stream asynchronously
                foreach (string file in files)
                {
                    using (FileStream inputStream = System.IO.File.OpenRead(file))
                    {
                        await inputStream.CopyToAsync(outputStream);
                    }
                }
            }

            if (Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.Delete(Path.GetDirectoryName(filePath), true);
            }
        }
    }
}
