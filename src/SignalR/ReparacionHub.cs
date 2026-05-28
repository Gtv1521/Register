using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Projections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FrameworkDriver_Api.src.SignalR
{
    [Authorize]
    public class ReparacionHub : Hub
    {
        private readonly IRegisters<RegisterModel, RegisterObsCliProjection> _registros;
        public ReparacionHub(IRegisters<RegisterModel, RegisterObsCliProjection> registers)
        {
            _registros = registers;
        }

        public override async Task OnConnectedAsync()
        {
            // Obtenemos el ID de la empresa desde el Token JWT del usuario
            var empresaId = Context.User?.FindFirst("EmpresaId")?.Value;

            if (!string.IsNullOrEmpty(empresaId))
            {
                // Metemos al usuario automáticamente en la "sala" de su empresa
                await Groups.AddToGroupAsync(Context.ConnectionId, empresaId);
            }

            await base.OnConnectedAsync();
        }

        public async Task RegistroCompleto(string data)
        {
            var empresaId = Context.User?.FindFirst("EmpresaId")?.Value;
            if (!string.IsNullOrEmpty(empresaId))
            {
                var response = await _registros.GetOneMasObservation(data);
                await Clients.Group(empresaId).SendAsync("RegistroCreado", response);
            }
        }

        
    }
}