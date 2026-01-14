using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkDriver_Api.src.Interfaces
{

    public interface IReadOne<T>
    {
        Task<T> GetByIdAsync(string id);
    }
    public interface IReadAll<T>
    {
        Task<IEnumerable<T>> GetAllAsync(int pageNumber, int pageSize);
    }
    public interface IReadAllId<T>
    {
        Task<IEnumerable<T>> GetAllIdAsync(string id, int pageNumber, int pageSize);
    }

    public interface IFiltered<T>
    {
        Task<IEnumerable<T>> GetByFilterAsync(string filter);
    }

    public interface ICreate<T>
    {
        Task<string> CreateAsync(T item);
    }
    public interface IUpdate<T>
    {
        Task<bool> UpdateAsync(string id, T item);
    }
    public interface IDelete
    {
        Task<bool> DeleteAsync(string id);
    }

    public interface ILoadMail<T>
    {
        Task<T?> LoadByEmailAsync(string email);
    }
    public interface ILoadPin<T>
    {
        Task<T?> LoadByPinAsync(int pin);
    }

    public interface ICrud<T> : IReadOne<T>, IReadAll<T>, ICreate<T>, IUpdate<T>, IDelete
    { }

    public interface ICrudWithLoad<T> : ICrud<T>, ILoadMail<T>
    { }

    public interface IAddFilter<T, P> : ICrud<T>
    {
        Task<IEnumerable<P>> FilterData(string text);
    }

    public interface ILoadAllId<T> : ICrud<T>, IReadAllId<T>
    { }
}