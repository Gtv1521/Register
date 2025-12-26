using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.Utils;
using MongoDB.Driver;

namespace FrameworkDriver_Api.src.Repositories
{
    public class RegisterRepository : ICrud<RegisterModel>
    {
        private readonly IMongoCollection<RegisterModel> _register;
        public RegisterRepository(Context context)
        {
            _register = context.GetCollection<RegisterModel>("Registers");
        }

        public Task<string> CreateAsync(RegisterModel item)
        {
            return _register.InsertOneAsync(item).ContinueWith(task => item.Id);
        }

        public Task<bool> DeleteAsync(string id)
        {
            var result = _register.DeleteOneAsync(register => register.Id == id);
            return result.ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public Task<IEnumerable<RegisterModel>> GetAllAsync(int pageNumber, int pageSize)
        {
            return _register.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync()
            .ContinueWith(task => (IEnumerable<RegisterModel>)task.Result);
        }

        public Task<RegisterModel> GetByIdAsync(string id)
        {
            var result = _register.FindAsync(register => register.Id == id);
            return result.ContinueWith(task => task.Result.FirstOrDefault());
        }

        public Task<bool> UpdateAsync(string id, RegisterModel item)
        {
            var result = _register.ReplaceOneAsync(register => register.Id == id, item);
            return result.ContinueWith(task => task.Result.ModifiedCount > 0);
        }
    }
}