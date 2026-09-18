using Application.DTOs;
using Application.Interfaces.Persistence;

namespace Application.UseCases.Subastas.ObtenerMisOfertas
{
    public class ObtenerMisOfertasQueryHandler : IQueryHandler<ObtenerMisOfertasQuery, IReadOnlyList<MiOfertaSubastaDto>>
    {
        private readonly ISubastaRepository _subastaRepository;

        public ObtenerMisOfertasQueryHandler(ISubastaRepository subastaRepository)
        {
            _subastaRepository = subastaRepository;
        }

        public async Task<IReadOnlyList<MiOfertaSubastaDto>> HandleAsync(ObtenerMisOfertasQuery query, CancellationToken ct = default)
        {
            return await _subastaRepository.ObtenerMisOfertasAsync(query.PostorId, ct);
        }
    }
}
