using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Interfaces
{
    public interface ICrud<T>
    {
        Task<IEnumerable<T>> GetAllAsync(int pageNumber, int pageSize); 
        Task<T> GetByIdAsync(string id);
        Task<string> CreateAsync(T item);
        Task<bool> UpdateAsync(string id, T item);
        Task<bool> DeleteAsync(string id);
    }
}