using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Dto;

using FrameworkDriver_Api.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrameworkDriver_Api.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]

    public class RegisterController : ControllerBase
    {
        private readonly RegisterService _registerService;

        private readonly ILogger<RegisterController> _logger;

        public RegisterController(RegisterService service, ILogger<RegisterController> logger)
        {
            _registerService = service;
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> AddRegister([FromBody] RegisterDTO register)
        {
            var result = await _registerService.AddRegisterAsync(register);
            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous] // entrada a anonima
        public async Task<IActionResult> GetAllRegisters([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _registerService.GetAllRegistersAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRegisterById(string id)
        {
            var result = await _registerService.GetRegisterByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("Filter/{filter}")]
        public async Task<IActionResult> FilterRegister(string filter)
        {
            if (filter == null) return BadRequest("Debe ingresar un dato para buscar");
            var response = await _registerService.Filter(filter);
            if (response.Count() == 0) return NotFound("No hay datos");
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRegister(string id, [FromBody] RegisterModel register)
        {
            var result = await _registerService.UpdateRegisterAsync(id, register);
            return Ok(result);
        }

    }
}