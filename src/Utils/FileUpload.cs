using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FrameworkDriver_Api.src.Models;
using Microsoft.Extensions.Options;

namespace FrameworkDriver_Api.src.Utils
{
    public class FileUpload
    {
        private readonly Cloudinary _cloudinary;

        public FileUpload(IOptions<CloudinaryModel> settings)
        {
            try
            {
                _cloudinary = new Cloudinary(settings.Value.Url);
                _cloudinary.Api.Secure = true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed connecting to Cloudinary: {ex.Message}");
            }
        }

        /// <summary>
        /// Upload image or video to Cloudinary
        /// </summary>
        public async Task<(string? Url, string? PublicId)> UploadMedia(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return (null, null);

            UploadResult uploadResult;

            // si es un video se sube mediante esta parte
            if (file.ContentType.StartsWith("video/"))
            {
                var videoParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                uploadResult = await _cloudinary.UploadAsync(videoParams);
            }
            // si es una imagen se sube aqui
            else if (file.ContentType.StartsWith("image/"))
            {
                var imageParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                uploadResult = await _cloudinary.UploadAsync(imageParams);
            }
            else
            {
                throw new ApplicationException("Only images and videos are allowed");
            }

            return (
                uploadResult.SecureUrl?.ToString(),
                uploadResult.PublicId
            );
        }


        // Delete image or video from Cloudinary

        public async Task<bool> DeleteMedia(string publicId, string resourceType = "image")
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = resourceType == "video"
                    ? ResourceType.Video
                    : ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
    }
}
