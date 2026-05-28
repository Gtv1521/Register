using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Interfaces
{
    public interface IEmail<T>
    {
        Task<T> SentMail(string email, string subject, string body);
    }
}