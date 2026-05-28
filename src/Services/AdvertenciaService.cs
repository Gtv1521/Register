using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace FrameworkDriver_Api.src.Services
{
    public class AdvertenciaService
    {
        private readonly ICrud<AdvertenciaModel> _repo;
        private readonly IHubContext<ReparacionHub> _hubContext;

        public AdvertenciaService(ICrud<AdvertenciaModel> repo, IHubContext<ReparacionHub> hubContext)
        {
            _repo = repo;
            _hubContext = hubContext;
        }

        /// <summary>
        /// se crea la advertencia
        /// </summary>
        /// <param name="item"></param>
        /// <param name="autor"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        public async Task<string> Create(AdvertenciasDto item, string autor, string company)
        {
            var insert = new AdvertenciaModel
            {
                IdCompany = item.IdCompany,
                Advertencia = item.Advertencia,
                Autor = autor,
            };

            var response = await _repo.CreateAsync(insert);
            await _hubContext.Clients.Group(company).SendAsync("AdvertenciaCreated", insert);
            return response;
        }

        /// <summary>
        ///  borra una advertencia 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="idCompany"></param>
        /// <returns></returns>
        public async Task<bool> Delete(string id, string idCompany)
        {
            var delete = await _repo.DeleteAsync(id);
            if (delete) await _hubContext.Clients.Group(idCompany).SendAsync("AdvertenciaDeleted", id);
            return delete;
        }

        /// <summary>
        /// trae una advertencia por el id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AdvertenciaModel> Get(string id)
        {
            return await _repo.GetByIdAsync(id);
        }

        /// <summary>
        /// trae todas las advertencias de la company
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="idCompany"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AdvertenciaModel>> GetAll(int page, int size, string idCompany)
        {
            return await _repo.GetAllAsync(page, size, idCompany);
        }

        /// <summary>
        /// actualiza datos de advertencia por el id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <param name="autor"></param>
        /// <param name="idCompany"></param>
        /// <returns></returns>
        public async Task<bool> Update(string id, AdvertenciasDto item, string autor, string idCompany)
        {
            var update = new AdvertenciaModel
            {
                Advertencia = item.Advertencia,
                Autor = autor
            };
            var response = await _repo.UpdateAsync(id, update);
            if (response) await _hubContext.Clients.Group(idCompany).SendAsync("AdvertenciaUpdated", new { id, advertencia = update.Advertencia, autor = update.Autor });
            return response;
        }
    }
}