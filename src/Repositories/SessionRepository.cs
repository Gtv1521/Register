using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.Utils;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace FrameworkDriver_Api.src.Repositories
{
    public class SessionRepository : ISession<SessionModel>
    {

        private readonly ICrudWithLoad<UserModel> _userRepository;
        private readonly IMongoCollection<SessionModel> _sessionCollection;
        public SessionRepository(ICrudWithLoad<UserModel> userRepository, Context context)
        {
            _userRepository = userRepository;
            _sessionCollection = context.GetCollection<SessionModel>("Sessions");
        }
        //  verifica si la sesion esta activa
        public async Task<bool> IsSessionActive(string sessionId)
        {
            return await _sessionCollection.Find(s => s.Id == sessionId && s.Status == "Active").AnyAsync();
        }

        // Inicia sesion
        public async Task<SessionModel> LogIn(SessionModel session)
        {
             
            return await _sessionCollection.InsertOneAsync(session).ContinueWith(task =>
            {
                return session;
            });
        }

        // Cierra sesion
        public async Task<bool> LogOut(string sessionId)
        {
            await _sessionCollection.UpdateOneAsync(
                Builders<SessionModel>.Filter.Eq(s => s.Id, sessionId),
                Builders<SessionModel>.Update
                    .Set(s => s.EndTime, DateTime.UtcNow)
                    .Set(s => s.Status, "Inactive")
            );
            return true;
        }

        // Crea un usuario y una sesion
        public async Task<SessionModel> SignIn(SessionModel user)
        {
            
            return await _sessionCollection.InsertOneAsync(user).ContinueWith(task =>
            {
                return new SessionModel
                {
                    UserId = user.Id.ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = "Active"
                };
            });
        }

        public async Task<long> CountAsync(string Id)
        {
            return await _sessionCollection.CountDocumentsAsync(user => user.UserId == Id);
        }
    }
}