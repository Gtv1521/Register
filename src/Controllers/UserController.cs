using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Services;
using Microsoft.AspNetCore.Mvc;
using ZstdSharp.Unsafe;

namespace FrameworkDriver_Api.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDto user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
                }
                var userId = await _userService.CreateUserAsync(new UserModel
                {
                    name = user.Name,
                    email = user.Email,
                    pin = user.Pin
                });
                _logger.LogInformation("User created with ID: {UserId}", userId);
                return CreatedAtAction(nameof(GetUserById), new { id = userId }, userId);

            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message, "Error creating user");
                return BadRequest(ex.Message);
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound("User not found");
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 10)
        {
            var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);
            if (users == null || !users.Any()) return NotFound("No users found");
            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDto user)
        {
            var updated = await _userService.UpdateUserAsync(id, new UserModel
            {
                name = user.Name,
                email = user.Email,
                pin = user.Pin
            });
            if (!updated) return NotFound("user not found");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted) return NotFound("User not found");
            return NoContent();
        }
    }
}