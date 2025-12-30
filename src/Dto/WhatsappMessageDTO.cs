using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Dto
{
    public class WhatsappMessageDTO
    {

        public string From { get; set; } = null!;
        public string To { get; set; } = null!;
        public string Type { get; set; } = null!;
        public SendZenTemplate Template { get; set; } = null!;

    }
    public class SendZenTemplate
    {
        public string name { get; set; } = null!;
        public string lang_code { get; set; } = null!;
    }
}