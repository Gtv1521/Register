using System;
using System.Collections.Generic;
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
    public class CompanyController : ControllerBase
    {
        private readonly CompanyService _companyService;

        public CompanyController(CompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpPost]
        // [RequestSizeLimit(5_000_000)] // 5MB limit
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateCompany([FromForm] CompanyDTO company)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            try
            {
                var companyId = await _companyService.CreateCompanyAsync(company);
                if (company == null) return BadRequest("Company data is null");
                return CreatedAtAction(nameof(GetCompanyById), new { id = companyId }, new { id = companyId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompanyById(string id)
        {
            if (id == null) return BadRequest("id no puede ser nulo");
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null) return NotFound("Company not found");
            return Ok(company);
        }

        [HttpGet("filter/{email}")]
        public async Task<IActionResult> BuscarCompany(string email)
        {
            if (email == null) return BadRequest("Envia un dato para buscar");
            try
            {
                var result = await _companyService.FilterCompany(email);
                if (result == null) return NotFound("Companies not found");
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCompanies([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1) return BadRequest("pageNumber and pageSize must be greater than 0");
                var companies = await _companyService.GetAllCompaniesAsync(pageNumber, pageSize);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(string id, [FromBody] CompanyModel company)
        {
            if (id == null) return BadRequest("id no puede ser nulo");
            if (!ModelState.IsValid) return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            try
            {
                var updated = await _companyService.UpdateCompanyAsync(id, company);
                if (!updated) return BadRequest("Company could not be updated");
                return Ok("Company updated successfully");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(string id)
        {
            if (id == null) return BadRequest("id no puede ser nulo");
            try
            {
                var deleted = await _companyService.DeleteCompanyAsync(id);
                if (!deleted) return BadRequest("Company could not be deleted");
                return Ok("Company deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
