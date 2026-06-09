using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.SignalR;
using FrameworkDriver_Api.src.Utils;
using Microsoft.AspNetCore.SignalR;

namespace FrameworkDriver_Api.src.Services
{
    public class CompanyService
    {
        private readonly IAddFilter<CompanyModel, CompanyModel> _company;
        private readonly FileUpload _cloudinary;
        private readonly IHubContext<ReparacionHub> _hubContext;
        private readonly ILogger<CompanyService> _logger;
        public CompanyService(
            IAddFilter<CompanyModel,
            CompanyModel> company,
            ILogger<CompanyService> logger,
            IHubContext<ReparacionHub> hubContext,
            FileUpload cloudinary
            )
        {
            _company = company;
            _cloudinary = cloudinary;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task<string> CreateCompanyAsync(CompanyDTO company)
        {
            // guarda la imagen en cloudinary
            string? Url = null;
            string? Id = null;
            if (company.Image != null) (Url, Id) = await _cloudinary.UploadMedia(company.Image, "Logo");

            //  se crea el componente con los datos 
            var companyModel = new CompanyModel
            {
                Name = company.Name,
                Email = company.Email,
                Phone = company.Phone,
                Address = company.Address,
                LogoId = Id!,
                LogoUrl = Url!,
                NIT = company.NIT
            };
            return await _company.CreateAsync(companyModel);
        }

        public async Task<CompanyModel> GetCompanyByIdAsync(string id)
        {
            return await _company.GetByIdAsync(id);
        }

        public async Task<IEnumerable<CompanyModel>> FilterCompany(string filter)
        {
            return await _company.FilterData(filter);
        }

        public async Task<IEnumerable<CompanyModel>> GetAllCompaniesAsync(int pageNumber, int pageSize)
        {
            return await _company.GetAllAsync(pageNumber, pageSize);
        }

        public async Task<bool> UpdateCompanyAsync(string id, CompanyDTO company, bool updateLogo)
        {
            _logger.LogInformation("Updating company with id: {CompanyId}, updateLogo: {UpdateLogo}", id, updateLogo);
            var response = await _company.GetByIdAsync(id);

            if (updateLogo && !string.IsNullOrEmpty(response.LogoId)) await _cloudinary.DeleteMedia(response.LogoId, "Logo");

            string? url = string.Empty;
            string? idpublic = string.Empty;
            if (updateLogo && company.Image != null) (url, idpublic) = await _cloudinary.UploadMedia(company.Image!, "Logo");
            var companyModel = new CompanyModel
            {
                Id = id,
                Name = company.Name,
                Email = company.Email,
                Phone = company.Phone,
                Address = company.Address,
                LogoId = updateLogo ? idpublic! : response.LogoId,
                LogoUrl = updateLogo ? url! : response.LogoUrl,
                NIT = company.NIT
            };

            var result = await _company.UpdateAsync(id, companyModel); 
            await _hubContext.Clients.All.SendAsync("CompanyUpdated", companyModel);
            return result;
        }

        public async Task<bool> DeleteCompanyAsync(string id)
        {
            return await _company.DeleteAsync(id);
        }
    }
}
