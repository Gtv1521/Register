using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;

namespace FrameworkDriver_Api.src.Services
{
    public class ClientService
    {
        private readonly IAddFilter<ClientModel, ClientModel> _client;
        public ClientService(IAddFilter<ClientModel, ClientModel> client)
        {
            _client = client;
        }
        public async Task<string> CreateClientAsync(ClientDTO client)
        {
            var clientModel = new ClientModel
            {
                Name = client.Name,
                Email = client.Email,
                Phone = client.Phone
            };
            return await _client.CreateAsync(clientModel);
        }
        public async Task<ClientModel> GetClientByIdAsync(string id)
        {
            return await _client.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ClientModel>> FilterClient(string filter)
        {
            return await _client.FilterData(filter);
        }
        public async Task<IEnumerable<ClientModel>> GetAllClientsAsync(int pageNumber, int pageSize)
        {
            return await _client.GetAllAsync(pageNumber, pageSize);
        }
        public async Task<bool> UpdateClientAsync(string id, ClientModel client)
        {
            return await _client.UpdateAsync(id, client);
        }
        public async Task<bool> DeleteClientAsync(string id)
        {
            return await _client.DeleteAsync(id);
        }

    }
}