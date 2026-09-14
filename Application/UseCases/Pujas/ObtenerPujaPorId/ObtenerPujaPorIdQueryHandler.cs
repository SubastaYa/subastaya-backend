using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Persistence;

namespace Application.UseCases.Pujas.ObtenerPujaPorId
{
    public class ObtenerPujaPorIdQueryHandler : IQueryHandler<ObtenerPujaPorIdQuery, PujaResumenDto?>
    {
        private readonly IPujaRepository _pujaRepository;

        public ObtenerPujaPorIdQueryHandler(IPujaRepository pujaRepository)
        {
            _pujaRepository = pujaRepository;
        }

        public async Task<PujaResumenDto?> HandleAsync(ObtenerPujaPorIdQuery query, CancellationToken ct = default)
        {
            return await _pujaRepository.ObtenerPorIdAsync(query.Id, ct);
        }
    }
}
