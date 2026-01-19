using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using FrameworkDriver_Api.src.Exceptions;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Utils;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;

namespace FrameworkDriver_Api.src.Services
{
    public class SessionService
    {
        private readonly IToken<UserModel> _tokenService;
        private readonly ICrudWithLoad<UserModel> _userRepository;
        private readonly ISession<SessionModel> _sessionRepository;
        private readonly EmailService _email;
        public SessionService(
            IToken<UserModel> tokenService,
            ICrudWithLoad<UserModel> userRepository,
            ISession<SessionModel> sessionRepository,
            EmailService email

            )
        {
            _tokenService = tokenService;
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _email = email;
        }

        // Inicia sesion
        public async Task<(SessionModel data, string Token)> LogIn(string email, string password)
        {
            var user = await _userRepository.LoadByEmailAsync(email);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            var verify = Argon2Hasher.Verify(password, user.Password);
            if (!verify)
                throw new UnauthorizedAccessException("Invalid credentials");

            var sessions = await _sessionRepository.CountAsync(user.Id);
            if (sessions >= 3)
                throw new MaxConnectionException("Máximas conexiones activas alcanzadas (3)");



            if (user != null)
            {
                var tokenRefresh = await _tokenService.GenerateRefreshToken(user.Id);
                var AccesToken = await _tokenService.GenerateToken(user, 1); // Token valido por 1 hora
                var session = new SessionModel
                {
                    UserId = user.Id,
                    StartTime = DateTime.UtcNow,
                    Status = "Active",
                    Token = tokenRefresh
                };

                // Aqui deberia guardarse la sesion en la base de datos
                return await _sessionRepository.LogIn(session).ContinueWith(task =>
                {
                    return (task.Result, AccesToken);
                });
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }
        }

        // Cierra sesion
        public async Task<bool> LogOut(string sessionId)
        {
            return await _sessionRepository.LogOut(sessionId);
        }
        // verifica si la sesion esta activa
        public async Task<bool> IsSessionActive(string sessionId)
        {
            return await _sessionRepository.IsSessionActive(sessionId);
        }

        // Crea un usuario y una sesion
        public async Task<(SessionModel data, string Token)> SignIn(UserModel user)
        {
            var hash = Argon2Hasher.Hash(user.Password);
            var response = await _userRepository.CreateAsync(new UserModel
            {
                Name = user.Name,
                Email = user.Email,
                Password = hash,
                Rol = user.Rol
            });

            // se crea tokens
            var tokenRefresh = await _tokenService.GenerateRefreshToken(response);
            var AccesToken = await _tokenService.GenerateToken(user, 1); // Token valido por 1 hora

            await _email.EnviarEmailAsync(
                    user.Email,
                    "Bienvenido a nuestro servicio",
                    $@"<h1>{user.Name}</h1>
                    <br>
                    <article>
                    Hola, Bienvenido !!! <br>
                    Este es el medio de comunicacion con el cliente donde se le
                    notifica novedades de lo que pasa con el servio que se le brinda.
                    </article>
                    <article>
                    Las funciones que hay son para facilitar tu vida...
                    </article>"
            );
            //  se crea la sesion en db
            return await _sessionRepository.SignIn(new SessionModel
            {
                UserId = response,
                StartTime = DateTime.UtcNow,
                Status = "Active",
                Token = tokenRefresh
            }).ContinueWith(task =>
            {
                if (task.Result == null)
                {
                    throw new UserException("User could not be created");
                }

                return (new SessionModel
                {
                    Id = task.Result.Id,
                    UserId = task.Result.UserId,
                    Token = tokenRefresh
                }, AccesToken);
            });
        }

        // Valida si el email ya existe
        public async Task<bool> ValidEmail(string email)
        {
            return await _userRepository.LoadByEmailAsync(email).ContinueWith(task =>
            {
                return task.Result != null;
            });
        }

        public async Task<(string tokenRefresh, string token)> updateToken(string tokenAntiguo, string idUser)
        {
            var user = await _userRepository.GetByIdAsync(idUser);
            var tokenNew = await _tokenService.GenerateRefreshToken(idUser);
            var tokenSistem = await _tokenService.GenerateToken(user, 1);
            var response = await _sessionRepository.UpdateTokenRefresh(tokenAntiguo, tokenNew, idUser);
            if (response != false) return (tokenNew, tokenSistem);
            throw new FailedException("Fallo actualizacion de token");
        }

        public async Task<IEnumerable<SessionModel>> OpenSessions(string idUser)
        {
            return await _sessionRepository.OpenSessions(idUser);
        }
    }
}