using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.Subastas.Queries.ObtenerCatalogo
{
    public class ObtenerCatalogoQueryHandler : IQueryHandler<ObtenerCatalogoQuery, IReadOnlyList<SubastaListDto>>
    {
        private readonly ISubastaRepository _subastaRepository;

        public ObtenerCatalogoQueryHandler(ISubastaRepository subastaRepository)
        {
            _subastaRepository = subastaRepository;
        }

        public async Task<IReadOnlyList<SubastaListDto>> HandleAsync(ObtenerCatalogoQuery query, CancellationToken ct = default)
        {
            return await _subastaRepository.ObtenerCatalogoAsync(
                query.CategoriaId,
                query.Estado,
                query.Busqueda,
                ct
            );
        }
    }
}
