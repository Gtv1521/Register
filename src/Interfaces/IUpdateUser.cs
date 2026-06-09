using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Interfaces
{
    public interface IUpdateUser
    {
        Task<bool> ActualizaPassword(string id, string pass);
        Task<bool> ActualizaName(string id, string name);
        Task<bool> ActualizaMail(string id, string mail);
    }
}