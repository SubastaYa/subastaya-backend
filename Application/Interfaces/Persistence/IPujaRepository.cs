using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IPujaRepository
    {
        Task AgregarAsync(Puja puja, CancellationToken ct = default);
        Task<PujaResumenDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<PujaResumenDto>> ObtenerPorSubastaIdAsync(int subastaId, CancellationToken ct = default);
    }
}
