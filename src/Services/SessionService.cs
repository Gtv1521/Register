using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Exceptions;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace FrameworkDriver_Api.src.Services
{
    public class SessionService
    {
        private readonly IToken<UserModel> _tokenService;
        private readonly ICrudWithLoad<UserModel> _userRepository;
        private readonly ISession<SessionModel> _sessionRepository;
        public SessionService(IToken<UserModel> tokenService, ICrudWithLoad<UserModel> userRepository, ISession<SessionModel> sessionRepository)
        {
            _tokenService = tokenService;
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
        }

        // Inicia sesion
        public async Task<(SessionModel data, string Token)> LogIn(int password)
        {
            var user = await _userRepository.LoadByPinAsync(password).ContinueWith(task =>
            {
                if (task.Result == null)
                {
                    throw new UnauthorizedAccessException("Invalid credentials");
                }
                var sesions = _sessionRepository.CountAsync(task.Result.Id).Result;
                if (sesions >= 3)
                {
                    throw new UnauthorizedAccessException("Maximas conexiones activas alcanzadas (3) ");
                }
                return task.Result;
            });
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
            var response = await _userRepository.CreateAsync(user);
            return await _sessionRepository.SignIn(new SessionModel
            {
                UserId = user.Id.ToString(),
                StartTime = DateTime.UtcNow,
                Status = "Active"
            }).ContinueWith(task =>
            {
                if (task.Result == null)
                {
                    throw new UserException("User could not be created");
                }
                var tokenRefresh = _tokenService.GenerateRefreshToken(task.Result.UserId);
                var AccesToken = _tokenService.GenerateToken(user, 1); // Token valido por 1 hora
                return (new SessionModel
                {
                    UserId = task.Result.UserId,
                    StartTime = task.Result.StartTime,
                    Status = task.Result.Status,
                    Id = task.Result.UserId,
                    Token = tokenRefresh.Result
                }, AccesToken.ToString());
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

        // Valida si el pin ya existe
        public Task<bool> ValidPin(int pin)
        {
            return _userRepository.LoadByPinAsync(pin).ContinueWith(task =>
            {
                return task.Result != null;
            });
        }
    }
}