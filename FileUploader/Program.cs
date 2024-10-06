
using FileUploader.Models;
using FileUploader.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;

namespace FileUploader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseUrls("http://0.0.0.0:7001");

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = long.MaxValue; // Large file size limit
            });

            ConfigureTranscodeOptions(builder);


            builder.Services.AddMemoryCache();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Enable static file serving
            app.UseStaticFiles();

            app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

            app.UseRouting();
            app.MapControllers();

            app.Run();
        }

        private static void ConfigureTranscodeOptions(IHostApplicationBuilder builder)
        {
            // Bind settings from appsettings.json
            builder.Services.Configure<TranscodeOption>(builder.Configuration.GetSection("TranscodeOption"));

            // Modify and extend the settings programmatically
            builder.Services.PostConfigure<TranscodeOption>(options =>
            {
                options.DefaultThreadCount = options.DefaultThreadCount == 0 ? 1 : options.DefaultThreadCount;
                options.GpuType = options.ForceDisableGpuUse ? GpuType.None : FFmpegHelper.DetectGpuType();
                options.DefaultVideoCodec = FFmpegHelper.GetTranscoder(options.GpuType);
                options.CurrenDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..\\");
                options.UploadDirectory = Path.Combine(options.CurrenDirectory, options.OutputDirectory);
            });
        }
    }
}
