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
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace FrameworkDriver_Api.src.Repositories
{
    public class RegisterRepository : IRegisters<RegisterModel, ListRegistersProjection, RegisterObsCliProjection>
    {
        private readonly IMongoCollection<RegisterModel> _register;
        private readonly IMongoCollection<ClientModel> _client;
        private readonly IMongoCollection<ObservationModel> _observation;
        private ILogger<RegisterRepository> _logger;
        public RegisterRepository(Context context, ILogger<RegisterRepository> logger)
        {
            _register = context.GetCollection<RegisterModel>("Registers");
            _client = context.GetCollection<ClientModel>("Clients");
            _observation = context.GetCollection<ObservationModel>("Observations");
            _logger = logger;
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

        public async Task<IEnumerable<RegisterObsCliProjection>> GetAllAsync(int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;

            var pipeline = _register.Aggregate()
            .As<BsonDocument>()

            // 🔹 Lookup observación
            .AppendStage<BsonDocument>(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Observations" },
                { "let", new BsonDocument("registerId", "$_id") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument("$expr",
                            new BsonDocument("$eq", new BsonArray { "$IdRegister", "$$registerId" })
                        )),
                        new BsonDocument("$limit", 1)
                    }
                },
                { "as", "Observation" }   // 👈 coincide con tu propiedad
            }))

            .AppendStage<BsonDocument>(new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$Observation" },
                { "preserveNullAndEmptyArrays", true }
            }))

            // 🔹 Lookup cliente
            .AppendStage<BsonDocument>(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Clients" },
                { "localField", "IdClient" },
                { "foreignField", "_id" },
                { "as", "Clients" }   // 👈 coincide con la propiedad
            }))

            .AppendStage<BsonDocument>(new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$Clients" },   // 👈 ahora sí correcto
                { "preserveNullAndEmptyArrays", true }
            }))
            .Skip(skip)
            .Limit(pageSize);

            var bsonResult = await pipeline.ToListAsync();

            var result = bsonResult
                .Select(doc => BsonSerializer.Deserialize<RegisterObsCliProjection>(doc))
                .ToList();
            return result;
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