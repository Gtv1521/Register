using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.Utils;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FrameworkDriver_Api.src.Repositories
{
    public class ObservationRepository : ILoadAllId<ObservationModel>
    {
        private readonly Context _context;
        public ObservationRepository(Context context)
        {
            _context = context;
        }

        public async Task<string> CreateAsync(ObservationModel item)
        {
            await _context.Observations.InsertOneAsync(item);
            return item.Id;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var delete = await _context.Observations.DeleteOneAsync(observation => observation.Id == id);
            return delete.DeletedCount > 0;
        }

        public async Task<bool> DeleteManyAsync(string id)
        {
            var delete = await _context.Observations.DeleteManyAsync(x => x.IdRegister == id);
            return delete.DeletedCount > 0;
        }

        public async Task<IEnumerable<ObservationModel>> FilterObs(string id, string filter)
        {
            var text = new BsonRegularExpression(filter, "i");

            var filtro = Builders<ObservationModel>.Filter.And(
                Builders<ObservationModel>.Filter.Eq(x => x.IdRegister, id),
                Builders<ObservationModel>.Filter.Or(
                    Builders<ObservationModel>.Filter.Regex(x => x.Description, text),
                    Builders<ObservationModel>.Filter.Regex(x => x.Id, text),
                    Builders<ObservationModel>.Filter.Regex(x => x.CreatedAt, text)
                )
            );

            return await _context.Observations.FindAsync(filtro).Result.ToListAsync();
        }

        public async Task<IEnumerable<ObservationModel>> GetAllAsync(int pageNumber, int pageSize, string? idCompany = null)
        {
            return await _context.Observations.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        }

        // Recibe el id de registro para cargar todos las actulizaciones que tiene.
        public async Task<IEnumerable<ObservationModel>> GetAllIdAsync(string id, int pageNumber, int pageSize)
        {
            return await _context.Observations
            .Find(x => x.IdRegister == id)
            .SortByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        }

        public async Task<ObservationModel> GetByIdAsync(string id)
        {
            return await _context.Observations.FindAsync(observation => observation.Id == id).Result.FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(string id, ObservationModel item)
        {
            if (!ObjectId.TryParse(id, out var objectId)) throw new ArgumentException("ID inválido");

            var filter = Builders<ObservationModel>.Filter.Eq(x => x.Id, id);
            //var update = Builders<ObservationModel>.Update.Set(x => x.Description, item.Description);

            var result = await _context.Observations.ReplaceOneAsync(filter, item);
            //await _context.Observations.UpdateOneAsync(filter, update).ContinueWith(task => task.Result.ModifiedCount > 0);
            return result.ModifiedCount > 0;
        }
    }
}