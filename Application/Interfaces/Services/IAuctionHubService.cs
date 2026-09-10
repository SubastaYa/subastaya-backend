namespace Application.Interfaces.Services
{
    public interface IAuctionHubService
    {
        Task BroadcastNuevaPujaAsync(int subastaId, decimal monto, string seudonimo, DateTime fechaPuja);
        Task BroadcastExtensionTiempoAsync(int subastaId, DateTime nuevaFechaFin);
    }
}
