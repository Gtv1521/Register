using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;

namespace FrameworkDriver_Api.src.Services
{
    public class UserService
    {
        private readonly ICrudWithLoad<UserModel> _user;
        public UserService(ICrudWithLoad<UserModel> user)
        {
            _user = user;
        }

        public async Task<string> CreateUserAsync(UserDto user)
        {
            return await _user.CreateAsync(new UserModel 
            { 
                name = user.Name, 
                email = user.Email, 
                pin = user.Pin
            });
        }
        public async Task<UserModel> GetUserByIdAsync(string id)
        {
            return await _user.GetByIdAsync(id);
        }
        public async Task<IEnumerable<UserModel>> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            return await _user.GetAllAsync(pageNumber, pageSize);
        }

        public async Task<bool> UpdateUserAsync(string id, UserDto user)
        {
            return await _user.UpdateAsync(id, new UserModel 
            { 
                name = user.Name, 
                email = user.Email, 
                pin = user.Pin
            });
        }
        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _user.DeleteAsync(id);
        }
    }
}