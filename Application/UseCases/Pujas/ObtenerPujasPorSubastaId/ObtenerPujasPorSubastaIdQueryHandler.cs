using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Persistence;

namespace Application.UseCases.Pujas.ObtenerPujasPorSubastaId
{
    public class ObtenerPujasPorSubastaIdQueryHandler : IQueryHandler<ObtenerPujasPorSubastaIdQuery, IReadOnlyList<PujaResumenDto>>
    {
        private readonly IPujaRepository _pujaRepository;

        public ObtenerPujasPorSubastaIdQueryHandler(IPujaRepository pujaRepository)
        {
            _pujaRepository = pujaRepository;
        }

        public async Task<IReadOnlyList<PujaResumenDto>> HandleAsync(ObtenerPujasPorSubastaIdQuery query, CancellationToken ct = default)
        {
            return await _pujaRepository.ObtenerPorSubastaIdAsync(query.SubastaId, ct);
        }
    }
}
