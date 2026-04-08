using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.Utils;
using MongoDB.Driver;

namespace FrameworkDriver_Api.src.Services
{
    public class MongoIndexInitializer : IIndexInitializer
    {
        private readonly Context _context;
        public MongoIndexInitializer(Context context)
        {
            _context = context;
        }

        public async Task InitializeIndexesAsync()
        {
            await CreateUniqueIndexesAsync(_context.Users, x => x.Email);
            // await CreateUniqueIndexesAsync(_context.Companies, x => x.Email);
        }


        private async Task CreateUniqueIndexesAsync<T>(IMongoCollection<T> collection, params Expression<Func<T, object>>[] fields)
        {
            if (fields == null || fields.Length == 0) return;

            var indexModels = fields.Select(f =>
                new CreateIndexModel<T>(
                    Builders<T>.IndexKeys.Ascending(f),
                    new CreateIndexOptions { Unique = true }
                )
            ).ToList();

            if (indexModels.Any())
                await collection.Indexes.CreateManyAsync(indexModels);
        }
    }
}