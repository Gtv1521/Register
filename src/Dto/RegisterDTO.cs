using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;

namespace FrameworkDriver_Api.src.Dto
{
    public class RegisterDTO
    {
        public string IdClient { get; set; } = string.Empty; // referencia al cliente
        public string IdCompany { get; set; } = string.Empty; // referencia a la empresa
        public string IdUser { get; set; } = string.Empty; // referencia al usuario que crea el registro
        public decimal Antisipo { get; set; } = 0;
        public decimal TotalPagar { get; set; } = 0;
        public string UrlRuta { get; set; } = string.Empty;
        public string RegistroNumber { get; set; } = string.Empty; // Número incremental formateado (ej: REG-000001)
    }
}