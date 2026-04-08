using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Exceptions;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.Utils;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens.Experimental;
using Microsoft.VisualBasic;
using MongoDB.Driver;
using ZstdSharp.Unsafe;

namespace FrameworkDriver_Api.src.Repositories
{
    public class UserRepository : ICrudWithLoad<UserModel>
    {
        private readonly Context _context;
        public UserRepository(Context context)
        {
            _context = context;
        }

        public async Task<string> CreateAsync(UserModel item)
        {
            if (!GetMailAsync(item.Email).Result.Item1)
            {
                await _context.Users.InsertOneAsync(item);
                return item.Id;
            }
            else
            {
                throw new EmailException("El mail ya existe");
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var delete = await _context.Users.DeleteOneAsync(user => user.Id == id);
            return delete.DeletedCount > 0;
        }

        public async Task<IEnumerable<UserModel>> GetAllAsync(int pageNumber, int pageSize, string? idCompany)
        {
            return await _context.Users.Find(x => x.IdCompany == idCompany)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        }

        public async Task<UserModel> GetByIdAsync(string id)
        {
            return await _context.Users.Find(user => user.Id == id).FirstOrDefaultAsync();
        }

        public async Task<UserModel?> LoadByEmailAsync(string email)
        {
            var response = await GetMailAsync(email);
            return response.Item2;
        }

        public async Task<bool> SaveTheme(string idUser, string theme)
        {
            var filter = Builders<UserModel>.Filter.Eq(x => x.Id, idUser);
            var update = Builders<UserModel>.Update.Set(x => x.Theme, theme);
            var response = await _context.Users.UpdateOneAsync(filter, update);
            return response.ModifiedCount > 0;
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

            var update = await _context.Users.UpdateOneAsync(user => user.Id == id, updatedUser);
            return update.ModifiedCount > 0;
        }

        //  valida que el mail no exista
        private async Task<(bool, UserModel?)> GetMailAsync(string mail)
        {
            var user = await _context.Users.Find(user => user.Email == mail).FirstOrDefaultAsync();
            return (user != null, user);
        }
    }
}