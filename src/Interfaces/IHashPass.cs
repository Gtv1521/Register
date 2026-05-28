using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Interfaces
{
    public interface IHashPass<T>
    {
        string Hash(T user, int password);
        bool Verify(T user, int password, int hashedPassword);
    }
}