using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Exceptions;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Services;
using FrameworkDriver_Api.src.Utils;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FrameworkDriver_Api.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ILogger<SessionController> _logger;
        private readonly SessionService _sessionService;
        public SessionController(
            ILogger<SessionController> logger,
            SessionService sessionService
            )
        {
            _logger = logger;
            _sessionService = sessionService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LogIn([FromBody] LoginRequest login)
        {
            if (!ModelState.IsValid) BadRequest(ModelState.Values);
            try
            {
                var (data, token) = await _sessionService.LogIn(login.Email, login.Password);
                _logger.LogInformation("User logged in successfully: {UserId}", data.UserId);


                Response.Cookies.Append("access_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(1)
                });

                Response.Cookies.Append("refresh_token", data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMonths(2)
                });

                return Ok(new
                {
                    idUser = data.UserId,
                    idSession = data.Id,
                    accessToken = token,
                    refreshToken = data.Token,
                });
            }
            catch (UnauthorizedAccessException uaEx)
            {
                _logger.LogWarning(uaEx.Message, "Unauthorized login attempt");
                return Unauthorized(new { message = uaEx.Message });
            }
            catch(MaxConnectionException ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message, "Error during login");
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    return BadRequest(new { message = ex.Message });
                }
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("logout/{sessionId}")]
        public async Task<IActionResult> LogOut(string sessionId)
        {
            try
            {
                var result = await _sessionService.LogOut(sessionId);
                return Ok(new { success = result });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("isActive/{sessionId}")]
        public async Task<IActionResult> IsSessionActive(string sessionId)
        {
            try
            {
                var isActive = await _sessionService.IsSessionActive(sessionId);
                return Ok(isActive);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error checking session status");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("openSessions/{IdUser}")]
        public async Task<IActionResult> OpenSessions(string IdUser)
        {
            try
            {
                var response = await _sessionService.OpenSessions(IdUser);
                return Ok(response);
            }
            catch (SystemException ex)
            {
                return BadRequest($"Fallo,{ex.Message}");
            }
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] UserDto user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values);
            try
            {
                (var data, var token) = await _sessionService.SignIn(new UserModel
                {
                    Name = user.Name,
                    Email = user.Email,
                    Password = user.Password,
                    Rol = user.Rol
                });

                Response.Cookies.Append("access_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(1)
                });

                Response.Cookies.Append("refresh_token", data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMonths(2)
                });
                return Ok(new
                {
                    idUser = data.UserId,
                    idSession = data.Id,
                    accessToken = token,
                    refreshToken = data.Token,
                });
            }
            catch (PinException pEx)
            {
                _logger.LogWarning(pEx.Message, "Pin conflict during sign-in");
                return BadRequest(pEx.Message);
            }
            catch (EmailException eEx)
            {
                _logger.LogWarning(eEx.Message, "Email conflict during sign-in");
                return BadRequest(eEx.Message);
            }
            catch (UserException uEx)
            {
                _logger.LogWarning(uEx.Message, "User creation failed during sign-in");
                return BadRequest(uEx.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message, "Error during sign-in");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTokenRefresh(string token, string id)
        {
            try
            {
                var response = await _sessionService.updateToken(token, id);
                _logger.LogInformation($" tokens = {response}");
                return Ok(new { response.token, response.tokenRefresh });
            }
            catch (FailedException)
            {
                return BadRequest(new { message = "Fallo en la actualización del token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en UpdateTokenRefresh");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}