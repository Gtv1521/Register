using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Projections;
using MongoDB.Bson;


namespace FrameworkDriver_Api.src.Services
{
    public class RegisterService
    {
        private readonly IRegisters<RegisterModel, ListRegistersProjection, RegisterObsCliProjection> _registerRepository;
        private readonly QrInterface _qrService;
        public RegisterService(
            IRegisters<RegisterModel, ListRegistersProjection, RegisterObsCliProjection> registerRepository,
            QrInterface qrService
            )
        {
            _registerRepository = registerRepository;
            _qrService = qrService;
        }

        public async Task<string> AddRegisterAsync(RegisterDTO register)
        {
            string Url = string.Empty; string Id = string.Empty;


            if (register.UrlRuta != string.Empty) (Url, Id) = await _qrService.GenerateQr(register.UrlRuta);

            return await _registerRepository.CreateAsync(new RegisterModel
            {
                IdClient = register.IdClient,
                StatusRegister = register.StatusRegister,
                CreatedAt = DateTime.UtcNow, // Guardar en UTC
                IdQr = Id,
                UrlQr = Url
            });
        }

        // hace un filtro del cliente y sale una lista de observaciones 
        public async Task<IEnumerable<ListRegistersProjection>> Filter(string filter)
        {
            return await _registerRepository.FilterData(filter);
        }

        public async Task<IEnumerable<RegisterObsCliProjection>> GetAllRegistersAsync(int pageNumber, int pageSize)
        {
            return await _registerRepository.GetAllAsync(pageNumber, pageSize);
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