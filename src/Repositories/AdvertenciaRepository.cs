using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.Utils;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FrameworkDriver_Api.src.Repositories
{
    public class AdvertenciaRepository : ICrud<AdvertenciaModel>
    {
        private readonly Context _context;
        public AdvertenciaRepository(Context context)
        {
            _context = context;
        }
        public async Task<string> CreateAsync(AdvertenciaModel item)
        {
            await _context.Advertencias.InsertOneAsync(item);
            return item.Id;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _context.Advertencias.DeleteOneAsync(x => x.Id == id);
            return response.DeletedCount > 0;
        }

        public async Task<IEnumerable<AdvertenciaModel>> GetAllAsync(int pageNumber, int pageSize, string? idCompany = null)
        {
            var filter = Builders<AdvertenciaModel>.Filter.Eq(x => x.IdCompany, idCompany);
            return await _context.Advertencias
                    .Find(filter)
                    .Limit(pageSize)
                    .Skip((pageNumber - 1) * pageSize)
                    .ToListAsync();
        }

        public async Task<AdvertenciaModel> GetByIdAsync(string id)
        {
            var filter = Builders<AdvertenciaModel>.Filter.Eq(x => x.Id, id);
            return await _context.Advertencias.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(string id, AdvertenciaModel item)
        {
            var filter = Builders<AdvertenciaModel>.Filter.Eq(x => x.Id, id);
            var update = Builders<AdvertenciaModel>.Update.Set(x => x.Advertencia, item.Advertencia).Set(x => x.Autor, item.Autor);
            var actualizado = await _context.Advertencias.UpdateOneAsync(filter, update);
            return actualizado.ModifiedCount > 0;
        }
    }
}