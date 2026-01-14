using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.Utils;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FrameworkDriver_Api.src.Repositories
{
    public class ClientRepository : IAddFilter<ClientModel, ClientModel>
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

        public async Task<bool> DeleteAsync(string id)
        {
            return await _clients.DeleteOneAsync(client => client.Id == id)
                .ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public async Task<IEnumerable<ClientModel>> FilterData(string text)
        {
            var regular = new BsonRegularExpression(text, "i");

            var filter = Builders<ClientModel>.Filter.Regex(x => x.Email, regular);
            return await _clients.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<ClientModel>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _clients.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        }

        public async Task<ClientModel> GetByIdAsync(string id)
        {
            return await _clients.FindAsync(client => client.Id == id)
                .ContinueWith(task => task.Result.FirstOrDefault());
        }

        public async Task<bool> UpdateAsync(string id, ClientModel item)
        {
            var filter = Builders<ClientModel>.Filter.Eq(c => c.Id, id);

            var update = Builders<ClientModel>.Update
                .Set(x => x.Email, item.Email)
                .Set(x => x.Name, item.Name)
                .Set(x => x.Phone, item.Phone);
         
            return await _clients.UpdateOneAsync(filter, update).ContinueWith(x => x.Result.ModifiedCount > 0);
        }
    }
}