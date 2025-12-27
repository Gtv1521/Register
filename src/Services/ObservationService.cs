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

namespace FrameworkDriver_Api.src.Services
{
    public class ObservationService
    {
        private readonly ICrud<ObservationModel> _observation;
        private readonly FileUpload _fileUpload;
        private readonly ILogger<ObservationService> _logger;


        public ObservationService(ICrud<ObservationModel> observation, FileUpload file, ILogger<ObservationService> logger)
        {
            _observation = observation;
            _fileUpload = file;
            _logger = logger;
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
                            var File = new PhotosModel
                            {
                                Id = idImage,
                                Photos = image,
                            };
                            FileData.Add(File);
                        }
                        // si existe un error al subir un archivo se notifica.
                        else
                        {
                            _logger.LogInformation("Error al subir el archivo " + photos);
                        }

                    }
                }
                var newObservation = new ObservationModel
                {
                    IdRegister = observation.IdRegister,
                    Type = observation.Type,
                    Description = observation.Description,
                    CreatedAt = DateTime.Now,
                    IdUser = observation.IdUser,
                    Photos = FileData,
                };
                return await _observation.CreateAsync(newObservation);

            }
            catch (System.Exception ex)
            {
                _logger.LogInformation("Ha ocurrido un error al crear la observacion" + ex);
                throw new Exception("Ha ocurrido el error " + ex.Message);
            }
        }
        public async Task<ObservationModel> GetClientByIdAsync(string id)
        {
            return await _observation.GetByIdAsync(id);
        }

    }
}