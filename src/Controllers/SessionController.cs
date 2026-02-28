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
using Microsoft.AspNetCore.Authorization;
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
        private readonly IWebHostEnvironment _env;
        public SessionController(
            ILogger<SessionController> logger,
            SessionService sessionService,
            IWebHostEnvironment env
            )
        {
            _logger = logger;
            _sessionService = sessionService;
            _env = env;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LogIn([FromBody] LoginRequest login)
        {
            if (!ModelState.IsValid) BadRequest(ModelState.Values);
            try
            {
                var isDevelopment = _env.IsDevelopment();
                var (data, token) = await _sessionService.LogIn(login.Email, login.Password);
                _logger.LogInformation("User logged in successfully: {UserId}", data.UserId);


                // Access Token (corto plazo - 15-60 minutos)
                Response.Cookies.Append("access_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(1), // Reducir a 15 minutos
                    Path = "/",
                    Domain = null,
                    IsEssential = true
                });

                // Refresh Token (largo plazo - almacenado en DB)
                Response.Cookies.Append("refresh_token", data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7), // Reducir a 7 días máximo
                    Path = "/",
                    Domain = null,
                    IsEssential = true
                });

                // Headers de seguridad adicionales
                Response.Headers.Append("X-Content-Type-Options", "nosniff");
                Response.Headers.Append("X-Frame-Options", "DENY");
                Response.Headers.Append("X-XSS-Protection", "1; mode=block");

                return Ok(new
                {
                    idUser = data.UserId,
                    idSession = data.Id,
                    idCompany = data.IdCompany,
                    accessToken = token,
                    refreshToken = data.Token,
                });
            }
            catch (UnauthorizedAccessException uaEx)
            {
                _logger.LogWarning(uaEx.Message, "Unauthorized login attempt");
                return Unauthorized(new { message = uaEx.Message });
            }
            catch (MaxConnectionException ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest(new { message = ex.Message, id = ex.Id });
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

                Response.Cookies.Delete("access_token", new CookieOptions
                {
                    Path = "/",
                    Secure = true,
                    SameSite = SameSiteMode.None
                });

                Response.Cookies.Delete("refresh_token", new CookieOptions
                {
                    Path = "/",
                    Secure = true,
                    SameSite = SameSiteMode.None
                });

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
        [Consumes("application/json", "multipart/form-data")]
        public async Task<IActionResult> SignIn([FromBody] UserDto user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            try
            {
                (var data, var token) = await _sessionService.SignIn(new UserModel
                {
                    Name = user.Name,
                    Email = user.Email,
                    Password = user.Password,
                    IdCompany = user.IdCompany,
                    Rol = user.Rol
                });

                // Access Token (corto plazo - 15-60 minutos)
                Response.Cookies.Append("access_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(1), // Reducir a 15 minutos
                    Path = "/",
                    Domain = null,
                    IsEssential = true
                });

                // Refresh Token (largo plazo - almacenado en DB)
                Response.Cookies.Append("refresh_token", data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7), // Reducir a 7 días máximo
                    Path = "/",
                    Domain = null,
                    IsEssential = true
                });

                // Headers de seguridad adicionales
                Response.Headers.Append("X-Content-Type-Options", "nosniff");
                Response.Headers.Append("X-Frame-Options", "DENY");
                Response.Headers.Append("X-XSS-Protection", "1; mode=block");

                return Ok(new
                {
                    idUser = data.UserId,
                    idSession = data.Id,
                    idCompany = data.IdCompany,
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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error during sign-in");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateTokenRefresh([FromBody] RefreshDto refreshDto)
        {
            var id = refreshDto.Id;

            if (string.IsNullOrEmpty(id)) return BadRequest("El id no debe estar en vacio");
            try
            {
                _logger.LogDebug("Refresh token endpoint called");

                var refreshToken = Request.Cookies["refresh_token"];

                _logger.LogDebug("Refresh token from cookies: {IsNull}",
                    string.IsNullOrEmpty(refreshToken) ? "NULL" : "PRESENT");

                if (string.IsNullOrEmpty(refreshToken))
                {
                    // También verificar el header por si el cliente lo envía ahí
                    var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                    if (authHeader != null && authHeader.StartsWith("Bearer "))
                    {
                        refreshToken = authHeader.Substring(7);
                        _logger.LogDebug("Refresh token from Authorization header");
                    }
                }

                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("No refresh token found in cookies or headers");
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Refresh token no encontrado",
                        error = "NO_REFRESH_TOKEN"
                    });
                }

                _logger.LogDebug("New tokens generated for user: {UserId}", id);

                // 5. ACTUALIZAR REFRESH TOKEN EN BASE DE DATOS
                (var refresh, var token) = await _sessionService.updateToken(refreshToken, id);

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Failed to update refresh token in database");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Error al actualizar sesión",
                        error = "DB_UPDATE_FAILED"
                    });
                }


                // Access Token (corto plazo - 15-60 minutos)
                Response.Cookies.Append("access_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddSeconds(10), // Reducir a 15 minutos
                    Path = "/",
                    Domain = null,
                    IsEssential = true
                });

                // Refresh Token (largo plazo - almacenado en DB)
                Response.Cookies.Append("refresh_token", refresh, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7), // Reducir a 7 días máximo
                    Path = "/",
                    Domain = null,
                    IsEssential = true
                });

                // Headers de seguridad adicionales
                Response.Headers.Append("X-Content-Type-Options", "nosniff");
                Response.Headers.Append("X-Frame-Options", "DENY");
                Response.Headers.Append("X-XSS-Protection", "1; mode=block");

                _logger.LogInformation("Tokens refreshed successfully for user: {UserId}", id);

                // 7. RESPONDER (sin enviar tokens en el cuerpo)
                return Ok(new
                {
                    success = true,
                    message = "Tokens refrescados exitosamente",
                    expiresIn = "2 dias"  // en segundos
                });
            }
            catch (SecurityTokenException stEx)
            {
                _logger.LogError(stEx, "Security token exception in refresh");
                ClearInvalidCookies();
                return Unauthorized(new
                {
                    success = false,
                    message = "Token de seguridad inválido",
                    error = "SECURITY_TOKEN_ERROR"
                });
            }
            catch (FailedException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in refresh token endpoint");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = "INTERNAL_SERVER_ERROR"
                });
            }
        }


        [HttpGet("verifyEmail/{email}")]
        public async Task<IActionResult> VerifyEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            try
            {
                var result = await _sessionService.ValidEmail(email);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error during email verification");
                return StatusCode(500, "Internal server error");
            }
        }
        private void ClearInvalidCookies()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = _env.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/"
            };

            Response.Cookies.Delete("access_token", cookieOptions);
            Response.Cookies.Delete("refresh_token", cookieOptions);
            Response.Cookies.Delete("session_active", cookieOptions);
        }
    }
}