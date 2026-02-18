using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;

namespace FrameworkDriver_Api.src.Dto
{
    public class RegisterDTO
    {
        public string IdClient { get; set; } = null!; // referencia al cliente
        public string IdUser { get; set; } = null!; // referencia al usuario que crea el registro
        public Status StatusRegister { get; set; } // estado del registro
        public string UrlRuta { get; set; } = string.Empty;
    }
}