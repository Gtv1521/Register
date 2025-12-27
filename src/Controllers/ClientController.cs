using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace FrameworkDriver_Api.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
                }
                if (client != null)
                {
                    var clientId = await _clientService.CreateClientAsync(client);
                    return CreatedAtAction(nameof(GetClientById), new { id = clientId }, clientId);
                }
                return BadRequest("Client data is null");

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
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null) return NotFound("Client not found");
            return Ok(client);
        }
        // Multiple points are missing here
    }
}