using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
<<<<<<< HEAD
using FrameworkDriver_Api.src.Dto;
=======
using FrameworkDriver_Api.Models;
>>>>>>> 79436e4 (update: funtions tokens)
using FrameworkDriver_Api.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace FrameworkDriver_Api.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
<<<<<<< HEAD
    public class RegisterController : ControllerBase
    {
        private readonly RegisterService _service;

        private readonly ILogger<RegisterController> _logger;

        public RegisterController(RegisterService service, ILogger<RegisterController> logger)
        {
            _service = service;
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> CreateRegister(RegisterDTO register)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

                if (register == null) return BadRequest("Client data is null");

                var registerId = await _service.CreateRegisterAsync(register);
                return Ok(registerId);

            }
            catch (System.Exception)
            {

                throw;
            }
=======

    // metodos para registro de servicios
    public class RegisterController : ControllerBase
    {
    
    private readonly RegisterService _registerService;

        public RegisterController(RegisterService registerService)
        {
            _registerService = registerService;
        }

        [HttpPost]
        public async Task<IActionResult> AddRegister([FromBody] RegisterModel register)
        {
            var result = await _registerService.AddRegisterAsync(register);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRegisters([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _registerService.GetAllRegistersAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRegisterById(string id)
        {
            var result = await _registerService.GetRegisterByIdAsync(id);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRegister(string id, [FromBody] RegisterModel register)
        {
            var result = await _registerService.UpdateRegisterAsync(id, register);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegister(string id)
        {
            var result = await _registerService.DeleteRegisterAsync(id);
            return Ok(result);
>>>>>>> 79436e4 (update: funtions tokens)
        }
    }
}