using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
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

        public ObservationService(
            ICrud<ObservationModel> observation,
            FileUpload file,
            ILogger<ObservationService> logger,
            WhatsappInterface whatsapp,
            ICrud<RegisterModel> register,
            ICrud<ClientModel> client)
        {
            _observation = observation;
            _fileUpload = file;
            _wh = whatsapp;
            _logger = logger;
            _register = register;
            _client = client;
        }

        public async Task<string> CreateObservationAsync(ObservationDTO observation)
        {
            try
            {
                var FileData = new List<PhotosModel>();
                // subida del archivo de imagen o video.
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
<<<<<<< HEAD
<<<<<<< HEAD
                                Photo = image,
=======
                                Photos = image,
>>>>>>> ef7d612 (update: funtions add branch Duvan)
=======
                                Photo = image,
>>>>>>> af9ae03 (feat: implement whatsapp utility)
                            });
                        }
                        // si existe un error al subir un archivo se notifica.
                        else
                        {
                            _logger.LogInformation("Error al subir el archivo " + photos);
                        }

                    }
                }
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> af9ae03 (feat: implement whatsapp utility)
                var register = await _register.GetByIdAsync(observation.IdRegister);
                var client = await _client.GetByIdAsync(register.IdClient);
                var responseWh = await _wh.SendMenssageAsync(observation.Description, client.Phone, FileData);

<<<<<<< HEAD
                //  retorna un id de objeto creado
                if (responseWh)
                {
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
                else
                {
                    throw new Exception("Ha ocurrido el error al crear la observacion");
                }


=======
=======
>>>>>>> af9ae03 (feat: implement whatsapp utility)
                //  retorna un id de objeto creado
                if (responseWh)
                {
<<<<<<< HEAD
                    IdRegister = observation.IdRegister,
                    Type = observation.Type,
                    Description = observation.Description,
                    CreatedAt = DateTime.Now,
                    IdUser = observation.IdUser,
                    Photos = FileData,
                });
>>>>>>> ef7d612 (update: funtions add branch Duvan)
=======
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
                else
                {
                    throw new Exception("Ha ocurrido el error al crear la observacion");
                }


>>>>>>> af9ae03 (feat: implement whatsapp utility)

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