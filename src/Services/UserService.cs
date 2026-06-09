using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.SignalR;
using FrameworkDriver_Api.src.Utils;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;

namespace FrameworkDriver_Api.src.Services
{
    public class UserService
    {
        private readonly ICrudWithLoad<UserModel> _user;
        private readonly EmailService _email;
        private readonly CompanyService _companyService;
        private readonly IHubContext<ReparacionHub> _hub;
        private readonly IUpdateUser _updateUser;

        public UserService(
            ICrudWithLoad<UserModel> user,
            IHubContext<ReparacionHub> hub,
            EmailService email,
            IUpdateUser updateUser,
            CompanyService companyService
            )
        {
            _user = user;
            _email = email;
            _hub = hub;
            _updateUser = updateUser;
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

        public async Task<bool> UpdateRol(string id, string rol, string company)
        {
            var response = await _user.UpdateRol(id, rol);
            if (response) await _hub.Clients.Group(company).SendAsync("UpdateRol", new { id, rol });
            return response;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _user.DeleteAsync(id);
        }

        public async Task<bool> SaveTheme(string idUser, string theme)
        {
            return await _user.SaveTheme(idUser, theme);
        }

        public async Task<bool> updateName(string id, string name)
        {
            var response = await _updateUser.ActualizaName(id, name);
            if (response) await _hub.Clients.User(id).SendAsync("ChangeName", name);


            return response;
        }

        public async Task<bool> UpdateMail(string id, string mail)
        {
            var response = await _updateUser.ActualizaMail(id, mail);
            if (response) await _hub.Clients.User(id).SendAsync("ChangeMail", mail);
            return response;
        }

        public async Task<bool> UpdatePassword(string id, string password)
        {
            var response = await _updateUser.ActualizaPassword(id, Argon2Hasher.Hash(password));
            if (response)
            {
                await _hub.Clients.User(id).SendAsync("Change Pass", new { message = "Actuallizado", state = 200 });
                var user = await GetUserByIdAsync(id);
                await _email.EnviarEmailAsync(user.Email, $@"Notificación.", $@"<h1>Cambio de contraseña</h1>
                    <br>
                    <article>
                    Hola, <strong>{user.Name}</strong> !!! <br>
                    Te notificamos que tu constraseña a sido cambiada con exito!.
                    </article>
                    ");
            }
            return response;
        }
    }
}