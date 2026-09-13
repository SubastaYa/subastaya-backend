using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.Subastas.ObtenerCatalogo
{
    public record ObtenerCatalogoQuery(
        int? CategoriaId = null,
        string? Estado = null,
        string? Busqueda = null,
        decimal? PrecioMin = null,
        decimal? PrecioMax = null,
        string? Orden = null,
        int? Page = null,
        int? PageSize = null
    ) : IQuery<IReadOnlyList<SubastaListDto>>;
}
