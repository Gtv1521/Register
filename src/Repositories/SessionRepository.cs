using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using FrameworkDriver_Api.src.Exceptions;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.Utils;
using Microsoft.VisualBasic;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FrameworkDriver_Api.src.Repositories
{
    public class SessionRepository : ISession<SessionModel>
    {
        private readonly IMongoCollection<SessionModel> _sessionCollection;
        public SessionRepository(Context context)
        {
            _sessionCollection = context.GetCollection<SessionModel>("Sessions");
        }
        //  verifica si la sesion esta activa
        public async Task<bool> IsSessionActive(string sessionId)
        {
            var filter = Builders<SessionModel>.Filter.And(
                Builders<SessionModel>.Filter.Eq(x => x.Id, sessionId),
                Builders<SessionModel>.Filter.Eq(x => x.Status, "Active")
            );

            var response = await _sessionCollection.Find(filter).FirstOrDefaultAsync();
            return response != null;
        }

        // Inicia sesion
        public async Task<SessionModel> LogIn(SessionModel session)
        {
            try
            {
                await _sessionCollection.InsertOneAsync(session);
                return session;
            }
            catch (Exception ex)
            {
                throw new Exception($"No se pudo inicar sesion {ex}");
            }
        }

        // Cierra sesion
        public async Task<bool> LogOut(string sessionId)
        {
            var session = Builders<SessionModel>.Filter.Eq(s => s.Id, sessionId);

            // trae usuario
            var user = await _sessionCollection.Find(session).FirstOrDefaultAsync();
            var ids = await SessionsClose(user.UserId);
            foreach (var (item, index) in ids.Select((value, i) => (value, i)))
            {
                if (index > 0) DeleteSessions(item.Id);
            }

            var update = Builders<SessionModel>.Update
                    .Set(s => s.EndTime, DateTime.UtcNow)
                    .Set(s => s.Status, "Inactive");
            var reponse = await _sessionCollection.UpdateOneAsync(session, update);
            return reponse.ModifiedCount > 0;
        }

        // Crea un usuario y una sesion
        public async Task<SessionModel> SignIn(SessionModel user)
        {
            try
            {
                await _sessionCollection.InsertOneAsync(user);
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("no se pudo iniciar session", ex);
            }
        }

        public async Task<long> CountAsync(string Id)
        {
            return await _sessionCollection.CountDocumentsAsync(user => user.UserId == Id && user.Status == "Active");
        }

        public async Task<bool> UpdateTokenRefresh(string token, string tokenNew, string id)
        {
            var filter = Builders<SessionModel>.Filter.And(
                Builders<SessionModel>.Filter.Eq(fl => fl.Token, token),
                Builders<SessionModel>.Filter.Eq(user => user.UserId, id));

            var busca = await _sessionCollection.Find(filter).FirstOrDefaultAsync();
            if (busca == null) return false;
            var update = Builders<SessionModel>.Update.Set(task => task.Token, tokenNew);
            var response = await _sessionCollection.UpdateOneAsync(filter, update);
            return response.ModifiedCount > 0;
        }

        public async Task<IEnumerable<SessionModel>> OpenSessions(string IdUser)
        {
            return await _sessionCollection.Find(x => x.UserId == IdUser).ToListAsync();
        }

        private async Task<IEnumerable<SessionModel>> SessionsClose(string id)
        {
            return await _sessionCollection.Find(x => x.UserId == id && x.Status == "Inactive")
                .Sort(Builders<SessionModel>.Sort.Ascending(r => r.EndTime))
                .ToListAsync();
        }

        private void DeleteSessions(string idUser)
        {
            _sessionCollection.DeleteOneAsync(x => x.Id == idUser);
        }
    }
}