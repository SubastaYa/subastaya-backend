using Application.DTOs;

namespace Application.UseCases.Categorias.ObtenerCategorias
{
    public record ObtenerCategoriasQuery : IQuery<IReadOnlyList<CategoriaDto>>;
}