using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Projections;
using Microsoft.AspNetCore.Routing.Tree;
using MongoDB.Bson;


namespace FrameworkDriver_Api.src.Services
{
    public class RegisterService
    {
        private readonly IRegisters<RegisterModel, ListRegistersProjection, RegisterObsCliProjection> _registerRepository;
        private readonly QrInterface _qrService;
        private readonly IUpdateQr _qr;
        public RegisterService(
            IRegisters<RegisterModel, ListRegistersProjection, RegisterObsCliProjection> registerRepository,
            QrInterface qrService,
            IUpdateQr qr
            )
        {
            _registerRepository = registerRepository;
            _qrService = qrService;
            _qr = qr;
        }

        public async Task<string> AddRegisterAsync(RegisterDTO register)
        {
            var nextRegistroNumber = await _registerRepository.GetNextRegistroNumberAsync();
            var result = await _registerRepository.CreateAsync(new RegisterModel
            {
                IdClient = register.IdClient,
                IdCompany = register.IdCompany,
                IdUser = register.IdUser,
                StatusRegister = register.StatusRegister,
                RegistroNumber = nextRegistroNumber,
                CreatedAt = DateTime.UtcNow, // Guardar en UTC
            });

            var BaseUrl = $"{register.UrlRuta}/{result}";

            string Url = string.Empty; string Id = string.Empty;

            if (register.UrlRuta != string.Empty) (Url, Id) = await _qrService.GenerateQr(BaseUrl);

            var qrResponse =  await UpdateQr(Url, Id, result);
            if (qrResponse == false) throw new Exception("No se pudo crear el codigo qr");
            return result;
            
        }

        public async Task<bool> UpdateQr(string url, string id, string idInsert)
        {
            return await _qr.UpdateQr(url, id, idInsert);
        }

        // hace un filtro del cliente y sale una lista de observaciones 
        public async Task<IEnumerable<ListRegistersProjection>> Filter(string filter)
        {
            return await _registerRepository.FilterData(filter);
        }

        public async Task<IEnumerable<RegisterObsCliProjection>> GetAllRegistersAsync(int pageNumber, int pageSize, string? idCompany = null)
        {
            return await _registerRepository.GetAllAsync(pageNumber, pageSize, idCompany);
        }

        public async Task<RegisterModel> GetRegisterByIdAsync(string id)
        {
            return await _registerRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateRegisterAsync(string id, RegisterModel register)
        {
            return await _registerRepository.UpdateAsync(id, register);
        }

        public async Task<bool> DeleteRegisterAsync(string id)
        {
            return await _registerRepository.DeleteAsync(id);
        }
    }
}