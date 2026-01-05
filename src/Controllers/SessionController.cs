using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Exceptions;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Services;
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
        public SessionController(ILogger<SessionController> logger, SessionService sessionService)
        {
            _logger = logger;
            _sessionService = sessionService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LogIn([FromBody] int password)
        {
            try
            {
                var (data, token) = await _sessionService.LogIn(password);
                _logger.LogInformation("User logged in successfully: {UserId}", data.UserId);
                return Ok(new { id = data.UserId, token });
            }
            catch (UnauthorizedAccessException uaEx)
            {
                _logger.LogWarning(uaEx.Message, "Unauthorized login attempt");
                return Unauthorized(uaEx.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message, "Error during login");
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    return BadRequest(ex.Message);
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
                return Ok(new { isActive });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error checking session status");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] UserDto user)
        {
            try
            {
                var (data, token) = await _sessionService.SignIn(new UserModel
                {
                    name = user.Name,
                    email = user.Email,
                    pin = user.Pin
                });
                return Ok(new { id = data.UserId, token });
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