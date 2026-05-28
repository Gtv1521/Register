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
using Microsoft.VisualBasic;

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
            if (string.IsNullOrEmpty(id)) return BadRequest("El id de registro es requerido");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _registerService.UpdateRegisterAsync(id, register);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation("Fallo al altualizar datos de registro: {mensaje}", ex.Message);
                throw;
            }
        }

        [HttpPut("total")]
        public async Task<IActionResult> UpdateTotel(decimal total, string IdRegister)
        {
            if (string.IsNullOrEmpty(IdRegister)) return BadRequest("Se requiere id de registro");
            if (total <= 0) return BadRequest("El valor total debe ser mayor a cero");
            try
            {
                var company = User.FindFirst("EmpresaId")?.Value;
                var response = await _registerService.UpdateTotal(total, IdRegister, company!);
                if (!response) return BadRequest("No se puedo actualizar el total");
                return Ok(new { success = response });
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation(ex.Message);
                throw;
            }

        }

        [HttpPut("antisipo")]
        public async Task<IActionResult> UpdateAntisipo(decimal antisipo, string idRegister)
        {
            if (string.IsNullOrEmpty(idRegister)) return BadRequest("El id de registro es necesario");
            if (antisipo <= 0) return BadRequest("el valor debe ser mayor a cero");
            try
            {
                var company = User.FindFirst("EmpresaId")?.Value;
                var response = await _registerService.UpdateAntisipo(antisipo, idRegister, company!);
                if (!response) return BadRequest("No se pudo actualizar el antisipo");
                return Ok(new { success = response });
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation("Hubo un error: {Mensaje}", ex.Message);
                throw;
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Super, Usuario")]
        public async Task<IActionResult> DeleteRegister(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("El Id de registro es requerido");
            try
            {
                var empresaId = User.FindFirst("EmpresaId")?.Value;
                var response = await _registerService.DeleteRegisterAsync(id, empresaId!);
                if (!response) return NotFound("No se encontro el objeto a eliminar");
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation("Fallo al eliminar registro: {Mensaje}", ex.Message);
                return Problem(ex.Message);
            }
        }

        [HttpGet("pdf/{id}")]
        public async Task<IActionResult> GeneratePDF(string id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("Se requiere id de registro");
            try
            {
                var result = await _registerService.GeneratePDFAsync(id, pageNumber, pageSize);
                return File(result, "application/pdf", $"test_{id}.pdf");

            }
            catch (System.Exception ex)
            {
                _logger.LogInformation("Fallo al traer pdf: {Message}", ex.Message);
                throw;
            }
        }

    }
}