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
    [Authorize]

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
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _registerService.AddRegisterAsync(register);
            return CreatedAtAction(nameof(GetRegisterById), new { id = result }, new { id = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRegisters([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? idCompany = null)
        {
            if (idCompany == null) return BadRequest("El idCompany es requerido");
            var result = await _registerService.GetAllRegistersAsync(pageNumber, pageSize, idCompany);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRegisterById(string id)
        {
            var result = await _registerService.GetRegisterByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("Filter/{filter}")]
        public async Task<IActionResult> FilterRegister(string filter, string idCompany, int page = 1, int size = 30)
        {
            if (filter == null) return BadRequest("Debe ingresar un dato para buscar");
            var response = await _registerService.Filter(filter, idCompany, page, size);
            if (response.Count() == 0) return NotFound("No hay datos");
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRegister(string id, [FromBody] RegisterModel register)
        {
            var result = await _registerService.UpdateRegisterAsync(id, register);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Super, Usuario")]
        public async Task<IActionResult> DeleteRegister(string id)
        {
            try
            {
                var empresaId = User.FindFirst("EmpresaId")?.Value;
                if (string.IsNullOrEmpty(id)) return BadRequest("el Id es requerido");
                var response = await _registerService.DeleteRegisterAsync(id, empresaId!);
                if (!response) return NotFound("No se encontro el objeto a eliminar");
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("pdf/{id}")]
        public async Task<IActionResult> GeneratePDF(string id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _registerService.GeneratePDFAsync(id, pageNumber, pageSize);
            return File(result, "application/pdf", $"test_{id}.pdf");
        }

    }
}