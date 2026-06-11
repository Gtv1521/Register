using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using DnsClient.Protocol;
using FrameworkDriver_Api.src.Exceptions;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualBasic;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FrameworkDriver_Api.src.Repositories
{
    public class SessionRepository : ISession<SessionModel>
    {
        private readonly Context _context;
        public SessionRepository(Context context)
        {
            _context = context;
        }
        //  verifica si la sesion esta activa
        public async Task<bool> IsSessionActive(string sessionId)
        {
            var filter = Builders<SessionModel>.Filter.And(
                Builders<SessionModel>.Filter.Eq(x => x.Id, sessionId),
                Builders<SessionModel>.Filter.Eq(x => x.Status, "Active")
            );

            var response = await _context.Sessions.Find(filter).FirstOrDefaultAsync();
            return response != null;
        }

        // Inicia sesion
        public async Task<SessionModel> LogIn(SessionModel session)
        {
            try
            {
                await _context.Sessions.InsertOneAsync(session);
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
            var user = await _context.Sessions.Find(session).FirstOrDefaultAsync();
            var ids = await SessionsClose(user.UserId);
            foreach (var (item, index) in ids.Select((value, i) => (value, i)))
            {
                if (index > 0) DeleteSessions(item.Id);
            }

            var update = Builders<SessionModel>.Update
                    .Set(s => s.EndTime, DateTime.UtcNow)
                    .Set(s => s.Status, "Inactive");
            var reponse = await _context.Sessions.UpdateOneAsync(session, update);
            return reponse.ModifiedCount > 0;
        }

        // Crea un usuario y una sesion
        public async Task<SessionModel> SignIn(SessionModel user)
        {
            try
            {
                await _context.Sessions.InsertOneAsync(user);
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("no se pudo iniciar session", ex);
            }
        }

        public async Task<long> CountAsync(string Id)
        {
            return await _context.Sessions.CountDocumentsAsync(user => user.UserId == Id && user.Status == "Active");
        }

        public async Task<bool> UpdateTokenRefresh(string token, string tokenNew, string id)
        {
            var filter = Builders<SessionModel>.Filter.And(
                Builders<SessionModel>.Filter.Eq(fl => fl.Token, token),
                Builders<SessionModel>.Filter.Eq(user => user.UserId, id));

            var busca = await _context.Sessions.Find(filter).FirstOrDefaultAsync();
            if (busca == null) return false;
            var update = Builders<SessionModel>.Update.Set(task => task.Token, tokenNew);
            var response = await _context.Sessions.UpdateOneAsync(filter, update);
            return response.ModifiedCount > 0;
        }

        public async Task<IEnumerable<SessionModel>> OpenSessions(string IdUser)
        {
            return await _context.Sessions.Find(x => x.UserId == IdUser).SortByDescending(x => x.StartTime).ToListAsync();
        }

        private async Task<IEnumerable<SessionModel>> SessionsClose(string id)
        {
            return await _context.Sessions.Find(x => x.UserId == id && x.Status == "Inactive")
                .Sort(Builders<SessionModel>.Sort.Ascending(r => r.EndTime))
                .ToListAsync();
        }

        private void DeleteSessions(string idUser)
        {
            _context.Sessions.DeleteOneAsync(x => x.Id == idUser);
        }

        public async Task<string> Added(string email, string token)
        {
            var insert = new RestartPassword
            {
                Email = email,
                Token = token,
            };
            await _context.RestartPassword.InsertOneAsync(insert);
            return insert.Id;

        }

        public async Task<bool> ValidaToken(string email, string token)
        {
            var filter = Builders<RestartPassword>.Filter.And(
                Builders<RestartPassword>.Filter.Eq(x => x.Email, email),
                Builders<RestartPassword>.Filter.Eq(x => x.Token, token)
            );

            var valida = await _context.RestartPassword.Find(filter).FirstOrDefaultAsync();

            if (DateTime.UtcNow >  valida.CreateAd.AddHours(1)  )
            {
                throw new TimeoutException("El tiempo del token expiro");
            }
            return true;
        }
    }
}