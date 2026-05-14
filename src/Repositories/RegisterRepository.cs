using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;
using CloudinaryDotNet.Actions;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Projections;
using FrameworkDriver_Api.Utils;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace FrameworkDriver_Api.src.Repositories
{
    public class RegisterRepository : IRegisters<RegisterModel, RegisterObsCliProjection>, IUpdateQr
    {
        private readonly Context _context;
        private ILogger<RegisterRepository> _logger;
        public RegisterRepository(Context context, ILogger<RegisterRepository> logger)
        {
            _context = context;
            _logger = logger;

            // Migrar registros existentes
            MigrateRegistroNumberAsync().ConfigureAwait(false);
        }

        private async Task MigrateRegistroNumberAsync()
        {
            try
            {
                // Buscar registros con RegistroNumber vacío o nulo
                var registrosToMigrate = await _context.Registers
                    .Find(x => x.RegistroNumber == null || x.RegistroNumber == string.Empty)
                    .ToListAsync();

                if (registrosToMigrate.Count > 0)
                {
                    foreach (var registro in registrosToMigrate)
                    {
                        var counter = await _context.Registers.CountDocumentsAsync(_ => true) + 1;
                        var newRegistroNumber = $"REG-{counter:D6}";

                        var filter = Builders<RegisterModel>.Filter.Eq(x => x.Id, registro.Id);
                        var update = Builders<RegisterModel>.Update.Set(x => x.RegistroNumber, newRegistroNumber);

                        await _context.Registers.UpdateOneAsync(filter, update);
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
            await _context.Registers.InsertOneAsync(item);
            return item.Id;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var delObservations = await _context.Observations.DeleteManyAsync(x => x.IdRegister == id);
            var delete = await _context.Registers.DeleteOneAsync(register => register.Id == id);
            return delete.DeletedCount > 0;
        }

        public async Task<IEnumerable<RegisterObsCliProjection>> FilterData(string? search, string idCompany, int page, int size)
        {
            if (string.IsNullOrWhiteSpace(idCompany))
                throw new Exception("Company required");

            var skip = (page - 1) * size;

            var pipeline = _context.Registers.Aggregate()

                // 🔹 1. filtro base (usa índice)
                .Match(x => x.IdCompany == idCompany)

                // 🔹 2. orden temprano (reduce ruido)
                .SortByDescending(x => x.CreatedAt)

                .As<BsonDocument>();

            // 🔹 3. LOOKUPS (solo si necesitas data relacional)
            pipeline = pipeline.AppendStage<BsonDocument>(new BsonDocument("$lookup", new BsonDocument
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
                { "as", "Observation" }
            }));

            pipeline = pipeline.AppendStage<BsonDocument>(new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$Observation" },
                { "preserveNullAndEmptyArrays", true }
            }));

            pipeline = pipeline.AppendStage<BsonDocument>(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Clients" },
                { "localField", "IdClient" },
                { "foreignField", "_id" },
                { "as", "Clients" }
            }));

            pipeline = pipeline.AppendStage<BsonDocument>(new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$Clients" },
                { "preserveNullAndEmptyArrays", true }
            }));

            // 🔍 4. FILTRO GLOBAL (MUY IMPORTANTE: después de joins)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var regex = new BsonRegularExpression(Regex.Escape(search), "i");

                pipeline = pipeline.AppendStage<BsonDocument>(
                    new BsonDocument("$match", new BsonDocument("$or", new BsonArray
                    {
                        new BsonDocument("RegistroNumber", regex),
                        new BsonDocument("Observation.Description", regex),
                        new BsonDocument("Clients.Name", regex)
                    }))
                );
            }

            // 🔹 5. paginación
            pipeline = pipeline
                .Skip(skip)
                .Limit(size);

            var data = await pipeline.ToListAsync();

            return data
                .Select(doc => BsonSerializer.Deserialize<RegisterObsCliProjection>(doc))
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public async Task<IEnumerable<RegisterObsCliProjection>> GetAllAsync(int pageNumber, int pageSize, string? idCompany = null)
        {
            var skip = (pageNumber - 1) * pageSize;

            if (idCompany == null) throw new Exception("El id de compañia no debe ser nulo");

            var pipeline = _context.Registers.Aggregate()
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

            return bsonResult
                .Select(doc => BsonSerializer.Deserialize<RegisterObsCliProjection>(doc))
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

        }

        public async Task<RegisterModel> GetByIdAsync(string id)
        {
            return await _context.Registers.FindAsync(register => register.Id == id).Result.FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(string id, RegisterModel item)
        {
            var result = await _context.Registers.ReplaceOneAsync(register => register.Id == id, item);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateQr(string urlImage, string idImage, string idInsert)
        {
            var filter = Builders<RegisterModel>.Filter.Eq(x => x.Id, idInsert);
            var update = Builders<RegisterModel>.Update.Set(x => x.IdQr, idImage).Set(x => x.UrlQr, urlImage);

            var result = await _context.Registers.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<string> GetNextRegistroNumberAsync()
        {
            var now = DateTime.Now;

            string datePart = now.ToString("yyMMddHHmmss");

            return $"REG-{datePart}";
        }

        public async Task<RegisterObsCliProjection> GetOneMasObservation(string id)
        {
            var pipeline = _context.Registers.Aggregate()
            .As<BsonDocument>();

            // 🔹 Filtrar por idCompany si se proporciona
            pipeline = pipeline.AppendStage<BsonDocument>(new BsonDocument("$match", new BsonDocument("_id", new ObjectId(id))));
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
            }));

            var doc = await pipeline.FirstOrDefaultAsync();

            if (doc == null) throw new Exception("No se encontro el registro recien creado");

            return BsonSerializer.Deserialize<RegisterObsCliProjection>(doc);

        }

        public async Task<bool> UpdateTotal(decimal total, string idRegister)
        {
            var filter = Builders<RegisterModel>.Filter.Eq(x => x.Id, idRegister);
            var update = Builders<RegisterModel>.Update.Set(x => x.TotalPagar, total);
            var response = await _context.Registers.UpdateOneAsync(filter, update);
            return response.ModifiedCount > 0;
        }

        public async Task<bool> UpdateAntisipo(decimal antisipo, string idRegister)
        {
            var filter = Builders<RegisterModel>.Filter.Eq(x => x.Id, idRegister);
            var update = Builders<RegisterModel>.Update.Set(x => x.Antisipo, antisipo);
            var response = await _context.Registers.UpdateOneAsync(filter, update);
            return response.ModifiedCount > 0;
        }
    }
}