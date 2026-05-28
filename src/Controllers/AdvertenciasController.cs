using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZstdSharp.Unsafe;

namespace FrameworkDriver_Api.src.Controllers
{
    [ApiController]
    [Authorize(Roles = "Administrador, Super")]
    [Route("api/[controller]")]
    public class AdvertenciasController : ControllerBase
    {
        private readonly AdvertenciaService _service;
        private readonly ILogger<AdvertenciasController> _logger;
        public AdvertenciasController(AdvertenciaService service, ILogger<AdvertenciasController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("se requiere el id");
            try
            {
                var response = await _service.Get(id);
                if (response == null) return NotFound("No se encontro advertencia");
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Hubo un error: {mensaje}", ex.Message);
                return Problem(
                    detail: "No se pudo procesar la solicitud en este momento. Intente más tarde.",
                    title: "Error Interno del Servidor",
                    statusCode: 500
                );
            }
        }

        [HttpGet("All/{idCompany}")]
        public async Task<IActionResult> GetAll(string idCompany, [FromQuery] int page = 1, [FromQuery] int size = 40)
        {
            if (string.IsNullOrEmpty(idCompany)) return BadRequest("Se necesita el idCompany");
            try
            {
                var response = await _service.GetAll(page, size, idCompany);
                if (response.Count() == 0) return NotFound("No se encontraron advertencias");
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Hubo un error en advertencias/All: {error}", ex.Message);
                return Problem(
                    detail: "No se pudo procesar la solicitud en este momento. Intente más tarde.",
                    title: "Error Interno del Servidor",
                    statusCode: 500
                );
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdvertenciasDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            try
            {
                var empresaId = User.FindFirst("EmpresaId")?.Value;
                var autor = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value ?? "Desconocido";
                var response = await _service.Create(dto, autor, empresaId!);
                _logger.LogInformation("Se creo una nueva advertencia con id: {id}", response);
                return Created("/Advertencias", new { id = response });
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Hubo un error en crear advertencia: {mensaje}", ex.Message);
                return Problem(
                    detail: "No se pudo procesar la solicitud en este momento. Intente más tarde.",
                    title: "Error Interno del Servidor",
                    statusCode: 500
                );
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] AdvertenciasDto dto)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("se requiere el id");
            if (!ModelState.IsValid) return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            try
            {
                var empresaId = User.FindFirst("EmpresaId")?.Value;
                var autor = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value ?? "Desconocido";
                var response = await _service.Update(id, dto, autor, empresaId!);
                if (!response) return NotFound("No se encontro advertencia para actualizar");
                return Ok(new { success = response });
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Hubo un error en actualizar advertencia: {mensaje}", ex.Message);
                return Problem(
                    detail: "No se pudo procesar la solicitud en este momento. Intente más tarde.",
                    title: "Error Interno del Servidor",
                    statusCode: 500
                );
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("se requiere el id");
            try
            {
                var empresaId = User.FindFirst("EmpresaId")?.Value;
                _logger.LogInformation("Intentando eliminar advertencia con id: {id} para la empresa: {company}", id, empresaId);
                var response = await _service.Delete(id, empresaId!);
                if (!response) return NotFound("No se encontro advertencia para eliminar");
                return Ok(new { success = response });
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Hubo un error en eliminar advertencia: {mensaje}", ex.Message);
                return Problem(
                    detail: "No se pudo procesar la solicitud en este momento. Intente más tarde.",
                    title: "Error Interno del Servidor",
                    statusCode: 500
                );
            }
        }
    }
}