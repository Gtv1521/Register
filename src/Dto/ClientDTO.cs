using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Dto
{
    public class ClientDTO
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!; // campo obligatorio
        public string Phone { get; set; } = null!; // campo obligatorio
    }
}