using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FrameworkDriver_Api.Utils
{
    public class Context
    {
        private readonly IMongoDatabase? _database;
        public Context(IOptions<DataContext> settings)
        {
            if (settings?.Value == null)
                throw new ArgumentNullException(nameof(settings), "Database settings cannot be null.");

            if (string.IsNullOrWhiteSpace(settings.Value.DefaultConnection))
                throw new ArgumentException("MongoDB connection string is required.", nameof(settings.Value.DefaultConnection));

            if (string.IsNullOrWhiteSpace(settings.Value.DatabaseName))
                throw new ArgumentException("MongoDB database name is required.", nameof(settings.Value.DatabaseName));

            var client = new MongoClient(settings.Value.DefaultConnection);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<UserModel> Users => GetCollection<UserModel>("Users");
        public IMongoCollection<CompanyModel> Companies => GetCollection<CompanyModel>("Companies");
        public IMongoCollection<RegisterModel> Registers => GetCollection<RegisterModel>("Registers");
        public IMongoCollection<ClientModel> Clients => GetCollection<ClientModel>("Clients");
        public IMongoCollection<ObservationModel> Observations => GetCollection<ObservationModel>("Observations");
        public IMongoCollection<SessionModel> Sessions => GetCollection<SessionModel>("Sessions");

        private IMongoCollection<T> GetCollection<T>(string name)
        {
            try
            {
                if (_database == null)
                {
                    throw new InvalidOperationException("Database connection is not initialized.");
                }
                return _database.GetCollection<T>(name);
            }
            catch (MongoException ex)
            {
                throw new Exception($"Could not get the collection: {name}.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the collection.", ex);
            }
        }
    }
}