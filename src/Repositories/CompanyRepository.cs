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
        private readonly Context _context;
        private readonly ILogger<CompanyRepository> _logger;

        public CompanyRepository(Context context
            , ILogger<CompanyRepository> logger
            )
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> CreateAsync(CompanyModel item)
        {
            await _context.Companies.InsertOneAsync(item);
            return item.Id;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _context.Companies.DeleteOneAsync(company => company.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<IEnumerable<CompanyModel>> FilterData(string text)
        {
            var regular = new BsonRegularExpression(text, "i");

            var filter = Builders<CompanyModel>.Filter.Regex(x => x.Email, regular);
            return await _context.Companies.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<CompanyModel>> GetAllAsync(int pageNumber, int pageSize, string? idCompany = null)
        {
            return await _context.Companies.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        }

        public async Task<CompanyModel> GetByIdAsync(string id)
        {
            return await _context.Companies.FindAsync(company => company.Id == id).Result.FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(string id, CompanyModel item)
        {
            _logger.LogInformation("Attempting to update company with id: {CompanyId}", id);
            if (!ObjectId.TryParse(id, out var objectId)) throw new ArgumentException("ID inválido");
            var filter = Builders<CompanyModel>.Filter.Eq(c => c.Id, item.Id);

            var result = await _context.Companies.ReplaceOneAsync(filter, item);
            return result.ModifiedCount > 0;
        }
    }
}
