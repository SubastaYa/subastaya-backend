using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.Subastas.Queries.ObtenerCatalogo
{
    public record ObtenerCatalogoQuery(
        int? CategoriaId = null,
        string? Estado = null,
        string? Busqueda = null
    ) : IQuery<IReadOnlyList<SubastaListDto>>;
}
