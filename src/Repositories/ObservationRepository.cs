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
    public class ObservationRepository : ILoadAllId<ObservationModel>
    {
        private readonly IMongoCollection<ObservationModel> _observations;
        public ObservationRepository(Context context)
        {
            _observations = context.GetCollection<ObservationModel>("Observations");
        }

        public async Task<string> CreateAsync(ObservationModel item)
        {
            return await _observations.InsertOneAsync(item).ContinueWith(task => item.Id);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _observations.DeleteOneAsync(observation => observation.Id == id)
                .ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public async Task<IEnumerable<ObservationModel>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _observations.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        }

        // Recibe el id de registro para cargar todos las actulizaciones que tiene.
        public async Task<IEnumerable<ObservationModel>> GetAllIdAsync(string id, int pageNumber, int pageSize)
        {
            return await _observations
            .Find(x => x.IdRegister == id)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        }

        public async Task<ObservationModel> GetByIdAsync(string id)
        {
            return await _observations.FindAsync(observation => observation.Id == id).ContinueWith(task => task.Result.FirstOrDefault());
        }

        public async Task<bool> UpdateAsync(string id, ObservationModel item)
        {
            var filter = Builders<ObservationModel>.Filter.Eq(x => x.Id, id);
            var update = Builders<ObservationModel>.Update.Set(x => x.Description, item.Description);

            return await _observations.UpdateOneAsync(filter, update).ContinueWith(task => task.Result.ModifiedCount > 0);
        }
    }
}