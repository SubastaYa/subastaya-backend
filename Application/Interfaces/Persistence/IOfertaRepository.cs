using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IOfertaRepository : IPujaRepository
    {
        Task AgregarOfertaAsync(Puja oferta, CancellationToken ct = default);
        Task<PujaResumenDto?> ObtenerOfertaPorIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<PujaResumenDto>> ObtenerOfertasPorSubastaIdAsync(int subastaId, CancellationToken ct = default);
    }
}
