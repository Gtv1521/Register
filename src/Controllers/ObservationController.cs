using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var client = await _observationService.GetClientByIdAsync(id);
            if (client == null) return NotFound("Client not found");
            return Ok(client);
        }

        //[HttpGet("{id}/{page}/{size}")]
        //public async Task<IActionResult> GetAllObservation(string id, int page = 1, int size = 10)
        //{
        //    try
        //    {
        //        if (id != null)
        //        {
        //            return await _observationService.get ();   
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        
        //    }
        //}
    }
}