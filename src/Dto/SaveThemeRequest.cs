using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Dto
{
    public class SaveThemeRequest
    {
        public string IdUser { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
    }
}