using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Persistence;

namespace Application.UseCases.Ofertas.ObtenerOfertaPorId
{
    public class ObtenerOfertaPorIdQueryHandler : IQueryHandler<ObtenerOfertaPorIdQuery, OfertaResumenDto?>
    {
        private readonly IOfertaRepository _ofertaRepository;

        public ObtenerOfertaPorIdQueryHandler(IOfertaRepository ofertaRepository)
        {
            _ofertaRepository = ofertaRepository;
        }

        public async Task<OfertaResumenDto?> HandleAsync(ObtenerOfertaPorIdQuery query, CancellationToken ct = default)
        {
            return await _ofertaRepository.ObtenerPorIdAsync(query.Id, ct);
        }
    }
}
