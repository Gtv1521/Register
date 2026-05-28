using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FrameworkDriver_Api.src.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FrameworkDriver_Api.src.Utils
{
    public class FileUpload
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<FileUpload> _logger;

        public FileUpload(IOptions<CloudinaryModel> settings, ILogger<FileUpload> logger)
        {
            _cloudinary = new Cloudinary(settings.Value.Url);
            _cloudinary.Api.Secure = true;
            _logger = logger;
        }

        /// <summary>
        /// Upload image or video to Cloudinary
        /// </summary>
        public async Task<(string? Url, string? PublicId)> UploadMedia(IFormFile file, string folder)
        {
            // if (file == null || file.Length == 0)
            //     throw new Exception("no se esta pasando un archivo");

            // UploadResult uploadResult;

            // // si es un video se sube mediante esta parte
            // if (file.ContentType.StartsWith("video/"))
            // {
            //     var videoParams = new VideoUploadParams
            //     {
            //         File = new FileDescription(file.FileName, file.OpenReadStream()),
            //         Folder = $"DataPqr/{folder}",
            //         UseFilename = true,
            //         UniqueFilename = false,
            //         Overwrite = true
            //     };

            //     uploadResult = await _cloudinary.UploadAsync(videoParams);
            // }
            // // si es una imagen se sube aqui
            // else if (file.ContentType.StartsWith("image/"))
            // {
            //     var imageParams = new ImageUploadParams
            //     {
            //         File = new FileDescription(file.FileName, file.OpenReadStream()),
            //         Folder = $"DataPqr/{folder}",
            //         UseFilename = true,
            //         UniqueFilename = false,
            //         Overwrite = true
            //     };

            //     uploadResult = await _cloudinary.UploadAsync(imageParams);
            // }
            // else
            // {
            //     throw new ApplicationException("Only images and videos are allowed");
            // }

            // return (
            //     uploadResult.SecureUrl?.ToString(),
            //     uploadResult.PublicId
            // );

            if (file == null || file.Length == 0) throw new Exception("Archivo vacío");

            // Determinamos si es video o imagen de forma más limpia
            bool isVideo = file.ContentType.StartsWith("video/");
            bool isImage = file.ContentType.StartsWith("image/");

            if (!isVideo && !isImage) throw new ApplicationException("Formato no permitido");

            // Comparten la misma base de parámetros
            var uploadParams = isVideo ? (RawUploadParams)new VideoUploadParams() : new ImageUploadParams();

            uploadParams.File = new FileDescription(file.FileName, file.OpenReadStream());
            uploadParams.Folder = $"DataPqr/{folder}";
            uploadParams.UseFilename = true;
            uploadParams.UniqueFilename = false;
            uploadParams.Overwrite = true;

            // Cloudinary detecta automáticamente el tipo si usas el método genérico o el específico
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            return (uploadResult.SecureUrl?.ToString(), uploadResult.PublicId);
        }


        // Delete image or video from Cloudinary

        public async Task<bool> DeleteMedia(string publicId, string resourceType = "image")
        {
            if (string.IsNullOrEmpty(publicId))
            {
                _logger.LogInformation("EL id de image es null");
                throw new NullReferenceException("el id de imagen es null");
            }
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = resourceType == "video"
                    ? ResourceType.Video
                    : ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }

        public async Task<bool> ComprobarEnlaceCloudinary(Cloudinary cloudinary)
        {
            try
            {
                var respuesta = await _cloudinary.PingAsync();
                return respuesta.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de enlace: {ex.Message}");
                return false;
            }
        }
    }
}
