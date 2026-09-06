using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.Subastas.Queries.ObtenerDetalle
{
    public class ObtenerSubastaPorIdQueryHandler : IQueryHandler<ObtenerSubastaPorIdQuery, SubastaDetalleDto?>
    {
        private readonly ISubastaRepository _subastaRepository;

        public ObtenerSubastaPorIdQueryHandler(ISubastaRepository subastaRepository)
        {
            _subastaRepository = subastaRepository;
        }

        public async Task<SubastaDetalleDto?> HandleAsync(ObtenerSubastaPorIdQuery query, CancellationToken ct = default)
        {
            return await _subastaRepository.ObtenerDetallePorIdAsync(query.Id, ct);
        }
    }
}
