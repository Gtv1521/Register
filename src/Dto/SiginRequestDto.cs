using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Dto
{
    public class SiginRequestDto : UserDto 
    {
        public required NavDataDto Data { get; set; }
    }
}