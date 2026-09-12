using Application.DTOs;

namespace Application.UseCases.Categorias.ObtenerCategorias
{
    public class ObtenerCategoriasQueryHandler : IQueryHandler<ObtenerCategoriasQuery, IReadOnlyList<CategoriaDto>>
    {
        private readonly ICategoriaRepository _categoriaRepository;
        public ObtenerCategoriasQueryHandler(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }
        public async Task<IReadOnlyList<CategoriaDto>> HandleAsync(ObtenerCategoriasQuery query, CancellationToken ct = default)
        {
            return await _categoriaRepository.ObtenerTodasAsync(ct);
        }
    }
}