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
    public class ObservationRepository : ICrud<ObservationModel>
    {
        private readonly IMongoCollection<ObservationModel> _observations;
        public ObservationRepository(Context context)
        {
            _observations = context.GetCollection<ObservationModel>("Observations");
        }

        public Task<string> CreateAsync(ObservationModel item)
        {
            return _observations.InsertOneAsync(item).ContinueWith(task => item.Id);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return _observations.DeleteOneAsync(observation => observation.Id == id)
                .ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public Task<IEnumerable<ObservationModel>> GetAllAsync(int pageNumber, int pageSize)
        {
            return _observations.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync()
            .ContinueWith(task => (IEnumerable<ObservationModel>)task.Result);
        }

        public Task<ObservationModel> GetByIdAsync(string id)
        {
            var result = _observations.FindAsync(observation => observation.Id == id);
            return result.ContinueWith(task => task.Result.FirstOrDefault());
        }

        public Task<bool> UpdateAsync(string id, ObservationModel item)
        {
            var result = _observations.ReplaceOneAsync(observation => observation.Id == id, item);
            return result.ContinueWith(task => task.Result.ModifiedCount > 0);
        }
    }
}