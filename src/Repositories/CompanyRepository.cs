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
    public class CompanyRepository : IAddFilter<CompanyModel, CompanyModel>
    {
        private readonly IMongoCollection<CompanyModel> _companies;
        
        public CompanyRepository(Context context)
        {
            _companies = context.GetCollection<CompanyModel>("Companies");
        }

        public async Task<string> CreateAsync(CompanyModel item)
        {
            return await _companies.InsertOneAsync(item).ContinueWith(task => item.Id);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _companies.DeleteOneAsync(company => company.Id == id)
                .ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public async Task<IEnumerable<CompanyModel>> FilterData(string text)
        {
            var regular = new BsonRegularExpression(text, "i");

            var filter = Builders<CompanyModel>.Filter.Regex(x => x.Email, regular);
            return await _companies.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<CompanyModel>> GetAllAsync(int pageNumber, int pageSize, string? idCompany = null)
        {
            return await _companies.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        }

        public async Task<CompanyModel> GetByIdAsync(string id)
        {
            return await _companies.FindAsync(company => company.Id == id)
                .ContinueWith(task => task.Result.FirstOrDefault());
        }

        public async Task<bool> UpdateAsync(string id, CompanyModel item)
        {
            var filter = Builders<CompanyModel>.Filter.Eq(c => c.Id, id);

            var update = Builders<CompanyModel>.Update
                .Set(c => c.Name, item.Name)
                .Set(c => c.Email, item.Email)
                .Set(c => c.Phone, item.Phone)
                .Set(c => c.Address, item.Address)
                .Set(c => c.NIT, item.NIT);

            var result = await _companies.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}
