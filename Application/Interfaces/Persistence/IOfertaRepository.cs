using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IOfertaRepository
    {
        Task AgregarAsync(Puja oferta, CancellationToken ct = default);
        Task<OfertaResumenDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<OfertaResumenDto>> ObtenerPorSubastaIdAsync(int subastaId, CancellationToken ct = default);
    }
}
