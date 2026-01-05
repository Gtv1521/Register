using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;

using FrameworkDriver_Api.src.Repositories;


namespace FrameworkDriver_Api.src.Services
{
    public class RegisterService
    {
        private readonly ICrud<RegisterModel> _registerRepository;
        public RegisterService(ICrud<RegisterModel> registerRepository)
        {
            _registerRepository = registerRepository;
        }

        public async Task<string> AddRegisterAsync(RegisterDto register)
        {
            return await  _registerRepository.CreateAsync(new RegisterModel
            {
                IdClient = register.IdClient,
                StatusRegister = register.StatusRegister,
                CreatedAt = DateTime.UtcNow // Guardar en UTC
            });
        }


        public async Task<IEnumerable<RegisterModel>> GetAllRegistersAsync(int pageNumber, int pageSize)
        {
            return await _registerRepository.GetAllAsync(pageNumber, pageSize);
        }

        public async Task<RegisterModel> GetRegisterByIdAsync(string id)
        {
            return await _registerRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateRegisterAsync(string id, RegisterModel register)
        {
            return await _registerRepository.UpdateAsync(id, register);
        }

        public async Task<bool> DeleteRegisterAsync(string id)
        {
            return await _registerRepository.DeleteAsync(id);
        }
    }
}