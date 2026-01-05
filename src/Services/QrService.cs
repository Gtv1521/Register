using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Utils;
using FrameworkDriver_Api.src.Utils.Interfaces;
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
            var Generator = new QRCodeGenerator();
            var data = Generator.CreateQrCode(
                content,
                QRCodeGenerator.ECCLevel.Q
            );
            var codigoQR = new QRCode(data);
            Bitmap bitmap = codigoQR.GetGraphic(20);
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            try
            {
                FormFile qr = new FormFile(
                    baseStream: ms,
                    baseStreamOffset: 0,
                    length: ms.Length,
                    name: "file",
                    fileName: "qr.png"
                );

                var FileUpload = await _cloudinary.UploadMedia(qr, "QR");
                if (FileUpload.Url != null && FileUpload.PublicId != null)
                {
                    return (FileUpload.Url, FileUpload.PublicId);
                }
                throw new Exception("error al guardar el qr");
            }
            catch (System.Exception)
            {

                throw new Exception("error al crear el qr");
            }

        }
    }
}