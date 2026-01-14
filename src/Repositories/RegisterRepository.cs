using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using System.Xml.Schema;
using CloudinaryDotNet.Actions;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Projections;
using FrameworkDriver_Api.Utils;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FrameworkDriver_Api.src.Repositories
{
    public class RegisterRepository : IAddFilter<RegisterModel, ListRegistersProjection>
    {
        private readonly IMongoCollection<RegisterModel> _register;
        private readonly IMongoCollection<ClientModel> _client;
        public RegisterRepository(Context context)
        {
            _register = context.GetCollection<RegisterModel>("Registers");
            _client = context.GetCollection<ClientModel>("Clients");
        }

        public Task<string> CreateAsync(RegisterModel item)
        {
            return _register.InsertOneAsync(item).ContinueWith(task => item.Id);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _register.DeleteOneAsync(register => register.Id == id)
                .ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public async Task<IEnumerable<ListRegistersProjection>> FilterData(string text)
        {
            var regex = new BsonRegularExpression(text, "i");
            var filter = Builders<ClientModel>.Filter.Regex(x => x.Name, regex);

            return await _client.Aggregate()
                .Match(filter)
                .Lookup(
                    foreignCollection: _register,
                    localField: c => c.Id,
                    foreignField: r => r.IdClient,
                    @as: (ListRegistersProjection x) => x.Registers
                )
                .ToListAsync();
        }

        public Task<IEnumerable<RegisterModel>> GetAllAsync(int pageNumber, int pageSize)
        {
            return _register.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync()
            .ContinueWith(task => (IEnumerable<RegisterModel>)task.Result);
        }

        public async Task<RegisterModel> GetByIdAsync(string id)
        {
            return await _register.FindAsync(register => register.Id == id)
                .ContinueWith(task => task.Result.FirstOrDefault());
        }

        public async Task<bool> UpdateAsync(string id, RegisterModel item)
        {
            return await _register.ReplaceOneAsync(register => register.Id == id, item)
                .ContinueWith(task => task.Result.ModifiedCount > 0);
        }
    }
}