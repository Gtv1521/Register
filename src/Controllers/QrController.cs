using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace FrameworkDriver_Api.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QrController : ControllerBase
    {
        private readonly QrInterface _qr;
        public QrController(QrInterface qr)
        {
            _qr = qr;
        }
        [HttpPost]
        public async Task<IActionResult> GenerateQr(string url)
        {
            try
            {
                var data = await _qr.GenerateQr(url);
                return Ok(data.Url);
            }
            catch (System.Exception)
            {
                throw new Exception("error al crear el qr en el servicio");
            }
        }
    }
}