using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZstdSharp.Unsafe;

namespace FrameworkDriver_Api.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class ClientController : ControllerBase
    {
        public readonly ClientService _clientService;

        public ClientController(ClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] ClientDTO client)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            try
            {
                var clientId = await _clientService.CreateClientAsync(client);
                if (client == null) return BadRequest("Client data is null");
                return CreatedAtAction(nameof(GetClientById), new { id = clientId }, clientId);

            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetClientById(string id)
        {
            if (id == null) return BadRequest("id no puede ser nulo");
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null) return NotFound("Client not found");
            return Ok(client);
        }


        [HttpGet("filter/{correo}")]
        public async Task<IActionResult> BuscarCliente(string correo)
        {
            if (correo == null) return BadRequest("Envia un dato para buscar");
            try
            {
                var filter = await _clientService.FilterClient(correo);
                if(filter.Count() == 0) return NotFound("Cliente no registrado");
                return Ok(filter);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(string id, [FromBody] ClientDTO client)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            try
            {
                var update = await _clientService.UpdateClientAsync(id, new ClientModel
                {
                    Name = client.Name,
                    Email = client.Email,
                    Phone = client.Phone
                });
                if (!update) return NotFound("No se encontro usuario para actualizar data");
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }
    }
}