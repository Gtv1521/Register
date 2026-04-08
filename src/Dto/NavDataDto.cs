using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Dto
{
    public class NavDataDto
    {
        public string Navegador { get; set; } = string.Empty;
        public string VersionNavegador { get; set; } = string.Empty;
        public string SistemaOperativo { get; set; } = string.Empty;
    }
}