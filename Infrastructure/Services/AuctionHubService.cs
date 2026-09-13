using Application.Interfaces.Services;
using Infrastructure.WebSockets;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services
{
    public class AuctionHubService : IAuctionHubService
    {
        private readonly IHubContext<AuctionHub> _hubContext;

        public AuctionHubService(IHubContext<AuctionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Emite el evento con el nuevo monto y postor a todos los usuarios conectados en la sala de la subasta
        public async Task BroadcastNuevaPujaAsync(int subastaId, decimal monto, string seudonimo, DateTime fechaPuja)
        {
            await _hubContext.Clients.Group(subastaId.ToString()).SendAsync("ReceiveNewBid", new
            {
                amount = monto,
                pseudonym = seudonimo,
                timestamp = fechaPuja
            });
        }

        // Emite la nueva fecha/hora de finalización cuando se activa la regla anti-sniping
        public async Task BroadcastExtensionTiempoAsync(int subastaId, DateTime nuevaFechaFin)
        {
            await _hubContext.Clients.Group(subastaId.ToString()).SendAsync("TimeExtended", new
            {
                newEndTime = nuevaFechaFin
            });
        }
    }
}
