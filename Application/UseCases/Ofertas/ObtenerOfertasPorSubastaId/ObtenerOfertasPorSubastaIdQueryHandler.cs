using Application.DTOs;

namespace Application.UseCases.Ofertas.ObtenerOfertasPorSubastaId
{
    public class ObtenerOfertasPorSubastaIdQueryHandler : IQueryHandler<ObtenerOfertasPorSubastaIdQuery, IReadOnlyList<OfertaResumenDto>>
    {
        private readonly IOfertaRepository _ofertaRepository;

        public ObtenerOfertasPorSubastaIdQueryHandler(IOfertaRepository ofertaRepository)
        {
            _ofertaRepository = ofertaRepository;
        }

        public async Task<IReadOnlyList<OfertaResumenDto>> HandleAsync(ObtenerOfertasPorSubastaIdQuery query, CancellationToken ct = default)
        {
            return await _ofertaRepository.ObtenerPorSubastaIdAsync(query.SubastaId, ct);
        }
    }
}
