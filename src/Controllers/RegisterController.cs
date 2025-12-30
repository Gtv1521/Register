using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace FrameworkDriver_Api.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        }
    }
}