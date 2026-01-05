using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;

namespace FrameworkDriver_Api.src.Dto
{
    public class ObservationDTO
    {
        public string IdRegister { get; set; } = null!; // referencia al registro dato unico del servicio    
        public ObservationType Type { get; set; }
        public string Description { get; set; } = null!;
        public string IdUser { get; set; } = null!; // nombre del tecnico que creo la observacion
        public List<IFormFile> Photos { get; set; } = null!; // lista de fotos asociadas a la observacion (hallasgos)
    }
}