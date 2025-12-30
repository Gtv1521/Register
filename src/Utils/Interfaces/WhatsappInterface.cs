using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Utils.Interfaces
{
    public interface WhatsappInterface
    {
        Task<bool> SendMenssageAsync(string Message, string Destiny);

    }
}