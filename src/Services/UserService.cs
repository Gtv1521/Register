using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;

namespace FrameworkDriver_Api.src.Services
{
    public class UserService
    {
        private readonly ICrud<UserModel> _user;
        public UserService(ICrud<UserModel> user)
        {
            _user = user;
        }

        public async Task<string> CreateUserAsync(UserModel user)
        {
            return await _user.CreateAsync(user);
        }
        public async Task<UserModel> GetUserByIdAsync(string id)
        {
            return await _user.GetByIdAsync(id);
        }
        public async Task<IEnumerable<UserModel>> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            return await _user.GetAllAsync(pageNumber, pageSize);
        }

        public async Task<bool> UpdateUserAsync(string id, UserModel user)
        {
            return await _user.UpdateAsync(id, user);
        }
        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _user.DeleteAsync(id);
        }
    }
}