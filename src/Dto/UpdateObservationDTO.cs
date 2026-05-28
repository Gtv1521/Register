using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Models;

namespace FrameworkDriver_Api.src.Dto
{
    public class UpdateObservationDTO
    {
        public ObservationType Type { get; set; }
        public string Description { get; set; } = null!;

        public bool NotificaEmail { get; set; }
        public bool NotificaWhatsapp { get; set; }
        public List<string>? DeletedPhotos { get; set; } // archivos borrados 
        public List<IFormFile>? NewPhotos { get; set; } // nuevas fotos 
    }
}