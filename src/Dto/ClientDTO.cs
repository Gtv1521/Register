using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Dto
{
    public class ClientDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty; // campo obligatorio
        [Required]
        public string Phone { get; set; } = string.Empty; // campo obligatorio
    }
}