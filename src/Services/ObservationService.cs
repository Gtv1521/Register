using System.Globalization;
using CloudinaryDotNet.Actions;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Projections;
using FrameworkDriver_Api.src.SignalR;
using FrameworkDriver_Api.src.Utils;
using FrameworkDriver_Api.src.Utils.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace FrameworkDriver_Api.src.Services
{
    public class ObservationService
    {
        private readonly ILoadAllId<ObservationModel> _observation;
        private readonly FileUpload _fileUpload;
        private readonly ILogger<ObservationService> _logger;
        private readonly IHubContext<ReparacionHub> _hubContext;
        private readonly WhatsappInterface _wh;
        private readonly IRegisters<RegisterModel, RegisterObsCliProjection> _register;
        private readonly IAddFilter<ClientModel, ClientModel> _client;
        private readonly EmailService _emailService;

        public ObservationService(
            ILoadAllId<ObservationModel> observation,
            FileUpload file,
            ILogger<ObservationService> logger,
            IHubContext<ReparacionHub> hubContext,
            WhatsappInterface whatsapp,
            IRegisters<RegisterModel, RegisterObsCliProjection> register,
            IAddFilter<ClientModel, ClientModel> client,
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
            _hubContext = hubContext;
        }

        public async Task<string> CreateObservationAsync(ObservationDTO observation)
        {
            try
            {
                var FileData = new List<PhotosModel>();

                if (observation.Photos != null && observation.Photos.Any())
                {
                    // 1. Creamos todas las tareas de subida al mismo tiempo (sin el await todavía)
                    var uploadTasks = observation.Photos
                        .Where(photo => photo != null)
                        .Select(async photo =>
                        {
                            try
                            {
                                var (url, id) = await _fileUpload.UploadMedia(photo, "observation");
                                return new { Success = url != null && id != null, Url = url, Id = id, PhotoName = photo.FileName };
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Excepción al subir {photo.FileName}: {ex.Message}");
                                return new { Success = false, Url = (string?)null, Id = (string?)null, PhotoName = photo.FileName };
                            }
                        });

                    // 2. Ejecutamos todas las tareas en paralelo y esperamos a que todas terminen
                    var results = await Task.WhenAll(uploadTasks);

                    // 3. Filtramos los resultados exitosos y llenamos tu lista
                    foreach (var res in results)
                    {
                        if (res.Success)
                        {
                            FileData.Add(new PhotosModel
                            {
                                Id = res.Id!,
                                Photo = res.Url!,
                            });
                        }
                        else
                        {
                            _logger.LogWarning($"No se pudo subir el archivo: {res.PhotoName}");
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
                                <h4>Notificacion de servicio.</h4>
                                <p><strong>Estado: </strong> {register.StatusRegister}</p>
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
                    //  *🔹 Actualización de Registro*
                    var mensajeWhatsapp = $"*Actualización de servicio* \n\n*Estado:* {register.StatusRegister}.\n\n*Observación:* \n{observation.Description}.\n\n*Cliente notificado*";
                    // envia mensaje a whatsapp
                    await _wh.SendMenssageAsync(mensajeWhatsapp, client.Phone, FileData);
                }


                var data = new ObservationModel
                {
                    IdRegister = observation.IdRegister,
                    Type = observation.Type,
                    Description = observation.Description,
                    CreatedAt = DateTime.Now,
                    IdUser = observation.IdUser,
                    Photos = FileData,
                };
                //  retorna un id de objeto creado
                var result = await _observation.CreateAsync(data);

                data.Id = result; // Asignamos el ID generado al modelo de observación
                // se actualiza el estado del registro dependiendo del tipo de observacion creada
                var response = await UpdateRegisterStatus(register, observation.Type);
                if (response) await _hubContext.Clients.Group(register.IdCompany).SendAsync("RegistroActualizado", register.Id);
                await _hubContext.Clients.Group(register.IdCompany).SendAsync("ObservacionCreada", data);
                return result;

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

        public async Task<ObservationModel> GetByIdAsync(string id)
        {
            return await _observation.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ObservationModel>> GetAllById(string IdRegister, int page, int size)
        {
            return await _observation.GetAllIdAsync(IdRegister, page, size);
        }

        public async Task<bool> Update(string id, UpdateObservationDTO item)
        {
            try
            {
                var ObDB = await _observation.GetByIdAsync(id);
                // borrar los id que fueron eliminados en el front
                if (item.DeletedPhotos != null && item.DeletedPhotos.Any())
                {
                    foreach (var data in item.DeletedPhotos)
                    {
                        if (await _fileUpload.DeleteMedia(data) == false) _logger.LogInformation("error al borrar el recurso de cloudinary" + data);
                        var photoRemove = ObDB.Photos.FirstOrDefault(c => c.Id == data);
                        if (photoRemove != null) ObDB.Photos.Remove(photoRemove);

                    }
                }
                //guardar las imagenes nuevas
                if (item.NewPhotos != null && item.NewPhotos.Any())
                {
                    foreach (var data in item.NewPhotos)
                    {
                        (string? image, string? idImage) = await _fileUpload.UploadMedia(data, "observation");
                        if (image != null && idImage != null)
                        {
                            ObDB.Photos.Add(new PhotosModel
                            {
                                Id = idImage,
                                Photo = image,
                            });
                        }
                        // si existe un error al subir un archivo se notifica.
                        else
                        {
                            _logger.LogInformation("Error al subir el archivo " + data);
                        }

                    }
                }
                // actualizar la descripcion y el tipo
                ObDB.Description = item.Description;

                var register = await _register.GetByIdAsync(ObDB.IdRegister);
                var client = await _client.GetByIdAsync(register.IdClient);
                //  se envia mensaje a correo
                if (item.NotificaEmail)
                {
                    var imagenesHtml = string.Join("<br>", ObDB.Photos.Select(url =>
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
                                <p><strong>Estado: </strong> {register.StatusRegister}</p>
                                <p><strong>Observación: </strong></p>
                                <p>{item.Description.Replace("\n", "<br>")}</p>

                                <h3>Evidencias:</h3>
                                {imagenesHtml}

                                <hr>
                                <p>Gracias por usar nuestro sistema.</p>
                            </body>
                            </html>"
                    );
                }


                //  se envia mensaje a whatsapp
                if (item.NotificaWhatsapp)
                {
                    //  *🔹 Actualización de Registro*
                    var mensajeWhatsapp = $"*Actualización de Registro*\n\n*Estado:* {register.StatusRegister}.\n\n*Observación:* \n{item.Description}.\n\n*Cliente notificado*";
                    // envia mensaje a whatsapp
                    await _wh.SendMenssageAsync(mensajeWhatsapp, client.Phone, ObDB.Photos);
                }

                return await _observation.UpdateAsync(id, ObDB);
            }
            catch (System.Exception)
            {

                throw new Exception("Error al actualizar la observacion.");
            }

        }

        public async Task<bool> DeleteXId(string id, string empresaId)
        {
            var images = await _observation.GetByIdAsync(id);
            if (images.Photos.Count() > 0)
            {
                images.Photos.ForEach(async x =>
                {
                    if (await _fileUpload.DeleteMedia(x.Id) == false) _logger.LogInformation("Error al borrar el recurso de cloudinary" + x.Id);
                    else _logger.LogInformation("Recurso de cloudinary eliminado correctamente: " + x.Id);
                });
            }

            var delete = await _observation.DeleteAsync(id);
            if (delete) await _hubContext.Clients.Groups(empresaId).SendAsync("ObservacionEliminada", id);
            _logger.LogInformation($"Observacion con id {id} eliminada: {delete}");
            return delete;
        }

        public async Task<bool> DeleteXRegister(string idRegister)
        {
            var images = await _observation.GetAllIdAsync(idRegister, 1, int.MaxValue);
            images.ToList().ForEach(async x =>
            {
                if (x.Photos.Count() > 0)
                {
                    x.Photos.ForEach(async p =>
                    {
                        if (await _fileUpload.DeleteMedia(p.Id) == false) _logger.LogInformation("Error al borrar el recurso de cloudinary" + p.Id);
                    });
                }
            });
            return await _observation.DeleteManyAsync(idRegister);
        }
        private async Task<bool> UpdateRegisterStatus(RegisterModel register, ObservationType newStatus)
        {
            register.StatusRegister = newStatus switch
            {
                ObservationType.Informacion => Status.EnProgreso,
                ObservationType.Cancelado => Status.Cancelado,
                ObservationType.Pendiente => Status.Pendiente,
                ObservationType.Solucion => Status.Completado,
                ObservationType.Entregado => Status.Entregado,

                _ => register.StatusRegister // Si no coincide, mantenemos el estado actual
            };
            var response = await _register.UpdateAsync(register.Id, register);
            await _hubContext.Clients.Group(register.IdCompany).SendAsync("RegistroActualizado", new { Id = register.Id, StatusRegister = register.StatusRegister as Status? });
            return response;
        }

    }
}