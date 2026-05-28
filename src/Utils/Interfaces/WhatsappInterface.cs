using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Models;

namespace FrameworkDriver_Api.src.Utils.Interfaces
{
    public interface WhatsappInterface
    {
        Task<bool> SendMenssageAsync(string Message, string Destiny, List<PhotosModel> image);

    }
}