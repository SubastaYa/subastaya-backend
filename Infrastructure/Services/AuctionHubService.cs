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

        public async Task BroadcastNuevaOfertaAsync(int subastaId, decimal monto, string seudonimo, DateTime fechaOferta, int compradorId)
        {
            await _hubContext.Clients.Group(subastaId.ToString()).SendAsync("ReceiveNewBid", new
            {
                amount = monto,
                pseudonym = seudonimo,
                timestamp = fechaOferta,
                buyerId = compradorId
            });
        }

        public async Task BroadcastExtensionTiempoAsync(int subastaId, DateTime nuevaFechaFin)
        {
            await _hubContext.Clients.Group(subastaId.ToString()).SendAsync("TimeExtended", new
            {
                newEndTime = nuevaFechaFin
            });
        }

        public async Task BroadcastSubastaFinalizadaAsync(int subastaId, string estado, string? ganadorSeudonimo, decimal? montoFinal)
        {
            await _hubContext.Clients.Group(subastaId.ToString()).SendAsync("AuctionClosed", new
            {
                auctionId = subastaId,
                status = estado,
                winnerPseudonym = ganadorSeudonimo,
                finalAmount = montoFinal
            });
        }
    }
}
