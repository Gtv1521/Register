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
        private readonly ICrud<RegisterModel> _registerRepo;
        private readonly ILogger<RegisterService> _logger;

        public RegisterService(ICrud<RegisterModel> crud, ILogger<RegisterService> logger)
        {
            _registerRepo = crud;
            _logger = logger;
        }

        public async Task<string> CreateRegisterAsync(RegisterDTO register)
        {
            try
            {
                var newRegister = new RegisterModel
                {
                    IdClient = register.IdClient,
                    StatusRegister = register.StatusRegister,
                    CreatedAt = DateTime.Now,
                };

                return await _registerRepo.CreateAsync(newRegister);

            }
            catch (System.Exception ex)
            {
                _logger.LogInformation("Ha ocurrido un error al crear el registro" + ex);

                throw new Exception("Ha ocurrido el error " + ex.Message);
            }
        }

    }
}