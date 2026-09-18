using Application.DTOs;
using Application.Interfaces.Persistence;

namespace Application.UseCases.Subastas.ObtenerMisPublicaciones
{
    public class ObtenerMisPublicacionesQueryHandler : IQueryHandler<ObtenerMisPublicacionesQuery, IReadOnlyList<MiPublicacionDto>>
    {
        private readonly ISubastaRepository _subastaRepository;

        public ObtenerMisPublicacionesQueryHandler(ISubastaRepository subastaRepository)
        {
            _subastaRepository = subastaRepository;
        }

        public async Task<IReadOnlyList<MiPublicacionDto>> HandleAsync(ObtenerMisPublicacionesQuery query, CancellationToken ct = default)
        {
            return await _subastaRepository.ObtenerMisPublicacionesAsync(query.VendedorId, ct);
        }
    }
}
