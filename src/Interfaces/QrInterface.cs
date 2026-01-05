using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Interfaces
{
    public interface QrInterface
    {
        Task<(string Url, string Id)> GenerateQr(string content);
    }
}