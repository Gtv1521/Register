using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZstdSharp.Unsafe;

namespace FrameworkDriver_Api.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        /// <remarks>
        /// Esta ruta agraga un Usuario al sistema. con el prodras interactuar de diferentes maneras con las funcionadidades que se ofrecen 
        /// <example>
        ///     
        ///     Ejemplo (Ingreso de data)
        /// 
        ///     {
        ///         "nombre": "Juan Pérez",
        ///         "email": "juan@example.com",
        ///         "telefono": "3001234567"
        ///         "pin": 1077
        ///     }
        /// </example>
        /// </remarks>
        /// <param name="user">Datos usuario</param>
        /// <returns>Devuelve el id de usuario creado</returns>
        /// <response code="200">Operación exitosa. Devuelve el recurso actualizado o creado.</response>
        /// <response code="201">Recurso creado exitosamente.</response>
        /// <response code="400">Solicitud inválida. Los datos enviados no cumplen con las validaciones.</response>
        /// <response code="401">No autenticado. Se requiere token JWT válido.</response>
        /// <response code="403">Acceso denegado. El usuario no tiene permisos para esta acción.</response>
        /// <response code="404">No encontrado. El recurso solicitado no existe.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> CreateUser([FromBody] UserDto user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
                }
                var userId = await _userService.CreateUserAsync(user);
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


        /// <summary>
        /// Obtiene un usuario por el id
        /// </summary>
        /// <param name="id">Entra el id de usuario</param>
        /// <returns>Trae las datos de usuario registrado</returns>
        /// <response code="200">Operación exitosa. Devuelve el recurso actualizado o creado.</response>
        /// <response code="201">Recurso creado exitosamente.</response>
        /// <response code="400">Solicitud inválida. Los datos enviados no cumplen con las validaciones.</response>
        /// <response code="401">No autenticado. Se requiere token JWT válido.</response>
        /// <response code="403">Acceso denegado. El usuario no tiene permisos para esta acción.</response>
        /// <response code="404">No encontrado. El recurso solicitado no existe.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound("User not found");
            return Ok(user);
        }

        /// <summary>
        /// Optiiene varios usuarios, se pasa 
        /// </summary>
        /// <param name="pageNumber">Numero de pagina</param>
        /// <param name="pageSize">Cantidad de usuarios a traer</param>
        /// <returns>Trae los usuario creados en el segmanto que se solicita.</returns>
        /// <response code="200">Operación exitosa. Devuelve el recurso actualizado o creado.</response>
        /// <response code="201">Recurso creado exitosamente.</response>
        /// <response code="400">Solicitud inválida. Los datos enviados no cumplen con las validaciones.</response>
        /// <response code="401">No autenticado. Se requiere token JWT válido.</response>
        /// <response code="403">Acceso denegado. El usuario no tiene permisos para esta acción.</response>
        /// <response code="404">No encontrado. El recurso solicitado no existe.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 10)
        {
            var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);
            if (users == null || !users.Any()) return NotFound("No users found");
            return Ok(users);
        }

        /// <summary>
        /// Hace una actualización de datos de usuario
        /// </summary>
        /// <param name="id">Entra id del usuario.</param>
        /// <param name="user">Datos de usuario a cambiar</param>
        /// <returns>Trae un estado 204</returns>
        /// <response code="200">Operación exitosa. Devuelve el recurso actualizado o creado.</response>
        /// <response code="201">Recurso creado exitosamente.</response>
        /// <response code="204">Recurso creado pero sin respuesta.</response>
        /// <response code="400">Solicitud inválida. Los datos enviados no cumplen con las validaciones.</response>
        /// <response code="401">No autenticado. Se requiere token JWT válido.</response>
        /// <response code="403">Acceso denegado. El usuario no tiene permisos para esta acción.</response>
        /// <response code="404">No encontrado. El recurso solicitado no existe.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]

        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDto user)
        {
            var updated = await _userService.UpdateUserAsync(id, user);
            if (!updated) return NotFound("user not found");
            return NoContent();
        }


        /// <summary>
        /// Borra un usuario 
        /// </summary>
        /// <param name="id">Entra id de usuario</param>
        /// <returns>Devuelve un NotContent Status(204)</returns>
        /// <response code="200">Operación exitosa. Devuelve el recurso actualizado o creado.</response>
        /// <response code="201">Recurso creado exitosamente.</response>
        /// <response code="400">Solicitud inválida. Los datos enviados no cumplen con las validaciones.</response>
        /// <response code="401">No autenticado. Se requiere token JWT válido.</response>
        /// <response code="403">Acceso denegado. El usuario no tiene permisos para esta acción.</response>
        /// <response code="404">No encontrado. El recurso solicitado no existe.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted) return NotFound("User not found");
            return NoContent();
        }
    }
}