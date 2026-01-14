using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Exceptions;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.Utils;
using Microsoft.VisualBasic;
using MongoDB.Driver;
using ZstdSharp.Unsafe;

namespace FrameworkDriver_Api.src.Repositories
{
    public class UserRepository : ICrudWithLoad<UserModel>
    {
        private readonly IMongoCollection<UserModel> _users;
        public UserRepository(Context context)
        {
            _users = context.GetCollection<UserModel>("Users");
        }

        public async Task<string> CreateAsync(UserModel item)
        {
            if (!GetMailAsync(item.Email).Result.Item1)
            {
                 return await _users.InsertOneAsync(item).ContinueWith(task => item.Id);
            }
            else
            {
                throw new EmailException("El mail ya existe");
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _users.DeleteOneAsync(user => user.Id == id)
                .ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public async Task<IEnumerable<UserModel>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _users.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync()
            .ContinueWith(task => (IEnumerable<UserModel>)task.Result);
        }

        public async Task<UserModel> GetByIdAsync(string id)
        {
            return await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
        }

        public async Task<UserModel?> LoadByEmailAsync(string email)
        {
            return await GetMailAsync(email).ContinueWith(task => task.Result.Item2);
        }

        // public async Task<UserModel?> LoadByPinAsync(int pin)
        // {
        //     return await UniquePinAsync(pin).ContinueWith(task => task.Result.objecto);
        // }

        public async Task<bool> UpdateAsync(string id, UserModel item)
        {

            var updatedUser = Builders<UserModel>.Update
                .Set(u => u.Name, item.Name)
                .Set(u => u.Email, item.Email)
                .Set(u => u.Password, item.Password);

            return await _users.UpdateOneAsync(user => user.Id == id, updatedUser)
                .ContinueWith(task => task.Result.ModifiedCount > 0);
        }

        //  valida que el mail no exista
        private async Task<(bool, UserModel?)> GetMailAsync(string mail)
        {
            return await _users.Find(user => user.Email == mail).FirstOrDefaultAsync()
            .ContinueWith(task =>
            {
                var user = task.Result; // O task.GetAwaiter().GetResult()
                return (user != null, user);
            });
        }

        // private async Task<(bool status, UserModel? objecto)> UniquePinAsync(int pin)
        // {
        //     return await _users.Find(u => u.pin == pin).FirstOrDefaultAsync()
        //     .ContinueWith(task =>
        //     {
        //         var user = task.Result; 
        //         return (user == null, user); // si es null el pin no existe
        //     });
        // }
    }
}