using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Interfaces
{
    public interface IUpdateQr
    {
        Task<bool> UpdateQr(string urlImage, string idImage, string idInsert);
    }
}