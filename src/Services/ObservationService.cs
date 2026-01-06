using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Utils;
using FrameworkDriver_Api.src.Utils.Interfaces;

namespace FrameworkDriver_Api.src.Services
{
    public class ObservationService
    {
        private readonly ICrud<ObservationModel> _observation;
        private readonly FileUpload _fileUpload;
        private readonly ILogger<ObservationService> _logger;
        private readonly WhatsappInterface _wh;
        private readonly ICrud<RegisterModel> _register;
        private readonly ICrud<ClientModel> _client;
        private readonly EmailService _emailService;

        public ObservationService(
            ICrud<ObservationModel> observation,
            FileUpload file,
            ILogger<ObservationService> logger,
            WhatsappInterface whatsapp,
            ICrud<RegisterModel> register,
            ICrud<ClientModel> client,
            EmailService emailService
            )
        {
            _observation = observation;
            _fileUpload = file;
            _wh = whatsapp;
            _logger = logger;
            _register = register;
            _emailService = emailService;
            _client = client;
        }

        public async Task<string> CreateObservationAsync(ObservationDTO observation)
        {
            try
            {
                var FileData = new List<PhotosModel>();
                // subida del archivo de imagen o video.
                if (observation.Photos != null)
                {
                    for (int i = 0; i < observation.Photos.Count; i++)
                    {
                        var photos = observation.Photos[i];
                        if (photos != null)
                        {
                            (string? image, string? idImage) = await _fileUpload.UploadMedia(photos, "observation");
                            if (image != null && idImage != null)
                            {
                                FileData.Add(new PhotosModel
                                {
                                    Id = idImage,
                                    Photo = image,
                                });
                            }
                            // si existe un error al subir un archivo se notifica.
                            else
                            {
                                _logger.LogInformation("Error al subir el archivo " + photos);
                            }

                        }
                    }
                }

                var register = await _register.GetByIdAsync(observation.IdRegister);
                var client = await _client.GetByIdAsync(register.IdClient);

                //  se envia mensaje a correo
                if (observation.NotificaEmail)
                {
                    var imagenesHtml = string.Join("<br>", FileData.Select(url =>
                        $"<img src=\"{url.Photo}\" alt=\"Evidencia\" style=\"max-width: 600px; height: auto; display: block; margin: 10px 0;\" />"
                    ));

                    await _emailService.EnviarEmailAsync(
                        client.Email,
                        "Actualizacion",
                        $@"
                            <html>
                            <body style='font-family: Arial, sans-serif;'>
                                <h2>Actualización de tu registro</h2>
                                <h4>Buen día</h4>
                                <p><strong>Estado:</strong> {register.StatusRegister}</p>
                                <p><strong>Observación:</strong></p>
                                <p>{observation.Description.Replace("\n", "<br>")}</p>

                                <h3>Evidencias:</h3>
                                {imagenesHtml}

                                <hr>
                                <p>Gracias por usar nuestro sistema.</p>
                            </body>
                            </html>"
                    );
                }
                //  se envia mensaje a whatsapp
                if (observation.NotificaWhatsapp)
                {
                    await _wh.SendMenssageAsync(observation.Description, client.Phone, FileData);
                }

                //  retorna un id de objeto creado
                return await _observation.CreateAsync(new ObservationModel
                {
                    IdRegister = observation.IdRegister,
                    Type = observation.Type,
                    Description = observation.Description,
                    CreatedAt = DateTime.Now,
                    IdUser = observation.IdUser,
                    Photos = FileData,
                });

            }
            catch (System.Exception ex)
            {
                _logger.LogInformation("Ha ocurrido un error al crear la observacion" + ex);
                throw new Exception("Ha ocurrido el error " + ex.Message);
            }
        }
        // 
        public async Task<ObservationModel> GetClientByIdAsync(string id)
        {
            return await _observation.GetByIdAsync(id);
        }
        // public async Task<ObservationModel> GetAll
    }
}