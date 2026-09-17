namespace Application.Interfaces.Services
{
    public interface IAuctionHubService
    {
        Task BroadcastNuevaOfertaAsync(int subastaId, decimal monto, string seudonimo, DateTime fechaOferta, int compradorId);
        Task BroadcastExtensionTiempoAsync(int subastaId, DateTime nuevaFechaFin);
        Task BroadcastSubastaFinalizadaAsync(int subastaId, string estado, string? ganadorSeudonimo, decimal? montoFinal);
    }
}
