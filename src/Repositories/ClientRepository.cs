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
        private readonly Context _context;
        public ClientRepository(Context context)
        {
            _context = context;
        }


        public async Task<string> CreateAsync(ClientModel item)
        {
            await _context.Clients.InsertOneAsync(item);
            return item.Id;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var delete = await _context.Clients.DeleteOneAsync(client => client.Id == id);
            return delete.DeletedCount > 0;
        }

        public async Task<IEnumerable<ClientModel>> FilterData(string text)
        {
            var regular = new BsonRegularExpression(text, "i");

            var filter = Builders<ClientModel>.Filter.Regex(x => x.Email, regular);
            return await _context.Clients.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<ClientModel>> GetAllAsync(int pageNumber, int pageSize, string? idCompany = null)
        {
            return await _context.Clients.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        }

        public async Task<ClientModel> GetByIdAsync(string id)
        {
            return await _context.Clients.FindAsync(client => client.Id == id)
                .Result.FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(string id, ClientModel item)
        {
            var filter = Builders<ClientModel>.Filter.Eq(c => c.Id, id);

            var update = Builders<ClientModel>.Update
                .Set(x => x.Email, item.Email)
                .Set(x => x.Name, item.Name)
                .Set(x => x.Phone, item.Phone);

            var result = await _context.Clients.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}