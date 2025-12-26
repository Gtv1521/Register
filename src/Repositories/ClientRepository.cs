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
    public class ClientRepository : ICrud<ClientModel>
    {
        private readonly IMongoCollection<ClientModel> _clients;
        public ClientRepository(Context context)
        {
            _clients = context.GetCollection<ClientModel>("Clients");
        }


        public async Task<string> CreateAsync(ClientModel item)
        {
            return await _clients.InsertOneAsync(item).ContinueWith(task => item.Id);
        }

        public Task<bool> DeleteAsync(string id)
        {
            var result = _clients.DeleteOneAsync(client => client.Id == id);
            return result.ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public async Task<IEnumerable<ClientModel>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _clients.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        }

        public Task<ClientModel> GetByIdAsync(string id)
        {
            var result = _clients.FindAsync(client => client.Id == id);
            return result.ContinueWith(task => task.Result.FirstOrDefault());
        }

        public Task<bool> UpdateAsync(string id, ClientModel item)
        {
            var result = _clients.ReplaceOneAsync(client => client.Id == id, item);
            return result.ContinueWith(task => task.Result.ModifiedCount > 0);
        }
    }
}