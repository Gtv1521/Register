using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Interfaces
{
    public interface ISession<T>
    {

        Task<T> SignIn(T user); // crea un usuario 
        Task<T> LogIn(T item); // inicia sesion
        Task<IEnumerable<T>> OpenSessions(string IdUser); // muestra los datos las sessiones abiertas
        Task<bool> LogOut(string sessionId); // cierra sesion
        Task<bool> UpdateTokenRefresh(string token, string tokenNew, string id); // actualiza token de refresh
        Task<bool> IsSessionActive(string sessionId); // verifica si la sesion esta activa
        Task<long> CountAsync(string Id); // cuenta sesiones por usuario
    }
}