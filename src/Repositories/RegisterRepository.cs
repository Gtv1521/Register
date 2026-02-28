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
    public class RegisterRepository : IRegisters<RegisterModel, ListRegistersProjection, RegisterObsCliProjection>, IUpdateQr
    {
        private readonly IMongoCollection<RegisterModel> _register;
        private readonly IMongoCollection<ClientModel> _client;
        private readonly IMongoCollection<ObservationModel> _observation;
        private static readonly Random _random = new Random();
        private ILogger<RegisterRepository> _logger;
        public RegisterRepository(Context context, ILogger<RegisterRepository> logger)
        {
            _register = context.GetCollection<RegisterModel>("Registers");
            _client = context.GetCollection<ClientModel>("Clients");
            _observation = context.GetCollection<ObservationModel>("Observations");
            _logger = logger;

            // Migrar registros existentes
            MigrateRegistroNumberAsync().ConfigureAwait(false);
        }

        private async Task MigrateRegistroNumberAsync()
        {
            try
            {
                // Buscar registros con RegistroNumber vacío o nulo
                var registrosToMigrate = await _register
                    .Find(x => x.RegistroNumber == null || x.RegistroNumber == string.Empty)
                    .ToListAsync();

                if (registrosToMigrate.Count > 0)
                {
                    foreach (var registro in registrosToMigrate)
                    {
                        var counter = await _register.CountDocumentsAsync(_ => true) + 1;
                        var newRegistroNumber = $"REG-{counter:D6}";

                        var filter = Builders<RegisterModel>.Filter.Eq(x => x.Id, registro.Id);
                        var update = Builders<RegisterModel>.Update.Set(x => x.RegistroNumber, newRegistroNumber);

                        await _register.UpdateOneAsync(filter, update);
                    }
                    _logger.LogInformation("Migración de RegistroNumber completada");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en migración de RegistroNumber: {ex.Message}");
            }
        }

        public async Task<string> CreateAsync(RegisterModel item)
        {
            return await _register.InsertOneAsync(item).ContinueWith(task => item.Id);
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

        public async Task<IEnumerable<RegisterObsCliProjection>> GetAllAsync(int pageNumber, int pageSize, string? idCompany = null)
        {
            var skip = (pageNumber - 1) * pageSize;

            if (idCompany == null) throw new Exception("El id de compañia no debe ser nulo");

            var pipeline = _register.Aggregate()
            .As<BsonDocument>();

            // 🔹 Filtrar por idCompany si se proporciona
            pipeline = pipeline.AppendStage<BsonDocument>(new BsonDocument("$match", new BsonDocument("IdCompany", new ObjectId(idCompany))));
            // ordena del ultimo al primero 
            pipeline = pipeline.AppendStage<BsonDocument>(new BsonDocument("$sort", new BsonDocument("CreatedAt", -1)));
            pipeline = pipeline
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
                .OrderByDescending(x => x.CreatedAt)
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

        public async Task<bool> UpdateQr(string urlImage, string idImage, string idInsert)
        {
            var filter = Builders<RegisterModel>.Filter.Eq(x => x.Id, idInsert);
            var update = Builders<RegisterModel>.Update.Set(x => x.IdQr, idImage).Set(x => x.UrlQr, urlImage);

            var result = await _register.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<string> GetNextRegistroNumberAsync()
        {
            var now = DateTime.Now;

            // 2. Formatear Año (últimos 2 dígitos) y Mes (con cero a la izquierda)
            // "yyMM" resultará en "2602" para febrero de 2026
            string datePart = now.ToString("yyMMdd");

            // 3. Generar un string aleatorio de 4 caracteres (Alfanumérico)
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] stringChars = new char[4];

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[_random.Next(chars.Length)];
            }

            string randomPart = new string(stringChars);

            // 4. Unir todo con el prefijo
            return $"REG{datePart}-{randomPart}";
        }

        public async Task<string> GenerateUniqueIdAsync()
        {
            string newId;
            bool exists;

            do
            {
                // 1. Generamos el ID
                newId = await GetNextRegistroNumberAsync();

                // 2. Verificamos en la DB si ya existe
                // Supongamos que usas Entity Framework o MongoDB Driver
                exists = await _register.Find(r => r.RegistroNumber == newId).AnyAsync();

            } while (exists); // Si existe, el bucle repite y genera otro

            return newId;
        }
    }
}