using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class OfertaRepository : PujaRepository, IOfertaRepository
    {
        public OfertaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task AgregarOfertaAsync(Puja oferta, CancellationToken ct = default) => AgregarAsync(oferta, ct);

        public Task<PujaResumenDto?> ObtenerOfertaPorIdAsync(int id, CancellationToken ct = default) => ObtenerPorIdAsync(id, ct);

        public Task<IReadOnlyList<PujaResumenDto>> ObtenerOfertasPorSubastaIdAsync(int subastaId, CancellationToken ct = default) => ObtenerPorSubastaIdAsync(subastaId, ct);
    }
}
