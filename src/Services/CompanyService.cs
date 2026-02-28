using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Utils;

namespace FrameworkDriver_Api.src.Services
{
    public class CompanyService
    {
        private readonly IAddFilter<CompanyModel, CompanyModel> _company;
        private readonly FileUpload _cloudinary;

        public CompanyService(
            IAddFilter<CompanyModel,
            CompanyModel> company,
            FileUpload cloudinary
            )
        {
            _company = company;
            _cloudinary = cloudinary;
        }

        public async Task<string> CreateCompanyAsync(CompanyDTO company)
        {
            // guarda la imagen en cloudinary
            string? Url = null;
            string? Id = null;
            if(company.Image != null)(Url, Id) = await _cloudinary.UploadMedia(company.Image, "Logo");

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

        public async Task<bool> UpdateCompanyAsync(string id, CompanyModel company)
        {
            return await _company.UpdateAsync(id, company);
        }

        public async Task<bool> DeleteCompanyAsync(string id)
        {
            return await _company.DeleteAsync(id);
        }
    }
}
