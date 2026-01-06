using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Utils;
using QRCoder;

namespace FrameworkDriver_Api.src.Services
{
    public class QrService : QrInterface
    {
        private readonly FileUpload _cloudinary;

        public QrService(FileUpload file)
        {
            _cloudinary = file;
        }

        public async Task<(string Url, string Id)> GenerateQr(string content)
        {
            try
            {
                var generator = new QRCodeGenerator();
                var data = generator.CreateQrCode(
                    content,
                    QRCodeGenerator.ECCLevel.Q
                );

                var qrCode = new PngByteQRCode(data);

                // PNG REAL
                byte[] qrBytes = qrCode.GetGraphic(20);

                // Stream correcto
                var stream = new MemoryStream(qrBytes);
                stream.Position = 0;

                IFormFile qrFile = new FormFile(
                    stream,
                    0,
                    qrBytes.Length,
                    "file",
                    "qr.png"
                )
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png"
                };

                var upload = await _cloudinary.UploadMedia(qrFile, "QR");

                if (upload.Url == null || upload.PublicId == null)
                    throw new Exception("Cloudinary no devolvió URL o PublicId");

                return (upload.Url, upload.PublicId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear o subir el QR", ex);
            }
        }
    }
}
