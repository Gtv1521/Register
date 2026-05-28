using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Dto
{
    public class AdvertenciasDto
    {
        [Required]
        public string IdCompany { get; set; } = string.Empty;

        [Required]
        public string Advertencia { get; set; } = string.Empty;
    }
}