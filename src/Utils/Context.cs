using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FrameworkDriver_Api.Utils
{
    public class Context
    {
        private readonly IMongoDatabase _database;
        public Context(IOptions<DataContext> settings)
        {
            try
            {
                if (settings.Value.ConnectionString != null && settings.Value.DatabaseName != null)
                {
                    var client = new MongoClient(settings.Value.ConnectionString);
                    _database = client.GetDatabase(settings.Value.DatabaseName);
                }

            }
            catch (MongoException ex)
            {
                throw new Exception("Could not connect to the database.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while initializing the database context.", ex);
            }
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}