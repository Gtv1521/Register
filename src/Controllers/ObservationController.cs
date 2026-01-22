using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
    // [Authorize]
    [Route("api/[controller]")]
    public class ObservationController : ControllerBase
    {
        private readonly ILogger<ObservationController> _logger;
        private readonly ObservationService _observationService;

        public ObservationController(ILogger<ObservationController> logger, ObservationService observationService)
        {
            _logger = logger;
            _observationService = observationService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateObservation([FromForm] ObservationDTO observation)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
                }
                if (observation != null)
                {
                    var clientId = await _observationService.CreateObservationAsync(observation);
                    return StatusCode(StatusCodes.Status200OK);
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
            var client = await _observationService.GetClientByIdAsync(id);
            if (client == null) return NotFound("Client not found");
            return Ok(client);
        }

        [HttpGet("{idRegister}/{page}/{size}")]
        public async Task<IActionResult> GetAllObservation(string idRegister, int page = 1, int size = 30)
        {
            try
            {
                if (idRegister == null) return BadRequest("El id no debe estar vacio");
                var response = await _observationService.GetAllById(idRegister, page, size);
                if (response.Count() == 0) return NotFound($"No hay observaciones para el registro: {idRegister}");
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> UpdateObservation(string id, [FromForm] UpdateObservationDTO item)
        {
            if (id == null) return BadRequest("El id no puede estar vacio");
            if (!ModelState.IsValid) return BadRequest(ModelState.Values.SelectMany(x => x.Errors));

            try
            {
                var response = await _observationService.Update(id, item);
                if (!response) return NotFound("No se encontro dato a actualizar");
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

    }
}