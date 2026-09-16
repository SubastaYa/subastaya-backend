namespace Application.Interfaces.Services
{
    public interface IAuctionHubService
    {
        Task BroadcastNuevaPujaAsync(int subastaId, decimal monto, string seudonimo, DateTime fechaPuja, int compradorId);
        Task BroadcastExtensionTiempoAsync(int subastaId, DateTime nuevaFechaFin);
        Task BroadcastSubastaFinalizadaAsync(int subastaId, string estado, string? ganadorSeudonimo, decimal? montoFinal);
    }
}
