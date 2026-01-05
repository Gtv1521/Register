using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Interfaces
{
    public interface IToken<T>
    {
        Task<string> GenerateToken(T user, int timeInHours);
        Task<string> GenerateRefreshToken(string id);
    }
}