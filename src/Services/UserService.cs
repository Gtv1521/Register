using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Utils;

namespace FrameworkDriver_Api.src.Services
{
    public class UserService
    {
        private readonly ICrudWithLoad<UserModel> _user;
        private readonly EmailService _email;
        private readonly CompanyService _companyService;

        public UserService(ICrudWithLoad<UserModel> user, EmailService email, CompanyService companyService)
        {
            _user = user;
            _email = email;
            _companyService = companyService;
        }

        public async Task<string> CreateUserAsync(UserDto user)
        {
            var nuevoUser = await _user.CreateAsync(new UserModel
            {
                Name = user.Name,
                Email = user.Email,
                Password = Argon2Hasher.Hash(user.Password),
                IdCompany = user.IdCompany,
                Rol = user.Rol
            });

            var name = (await _companyService.GetCompanyByIdAsync(user.IdCompany)).Name;

            await _email.EnviarEmailAsync(user.Email, $@"Bienvenido a {name}", $@"<h1>{user.Name}</h1>
                    <br>
                    <article>
                    Hola, Te damos la bienvenida !!! <br>
                    Este es el medio de comunicacion con el cliente donde se le notifica novedades de lo que pasa con el servio que se le brinda.
                    </article>
                    <strong>
                    Tus credenciales..
                    </strong>

                    <br>
                    Email: {user.Email}
                    <br>
                    Password: {user.Password}

                    <article> 
                    Te recomandamos cambiar la contraseña por una personalizada, para eso puedes ir a tu perfil y editar tus datos. 
                    </article> 
                    ");

            return nuevoUser;
        }
        public async Task<UserModel> GetUserByIdAsync(string id)
        {
            return await _user.GetByIdAsync(id);
        }
        public async Task<IEnumerable<UserModel>> GetAllUsersAsync(string company, int pageNumber, int pageSize)
        {
            return await _user.GetAllAsync(pageNumber, pageSize, company);
        }

        public async Task<bool> UpdateUserAsync(string id, UserDto user)
        {
            return await _user.UpdateAsync(id, new UserModel
            {
                Name = user.Name,
                Email = user.Email,
                Password = Argon2Hasher.Hash(user.Password),
                Rol = user.Rol
            });
        }
        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _user.DeleteAsync(id);
        }

        public async Task<bool> SaveTheme(string idUser, string theme)
        {
            return await _user.SaveTheme(idUser, theme);
        }
    }
}