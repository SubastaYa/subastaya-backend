using Application.DTOs;

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
        int? PageSize = null,
        int? VendedorId = null,
        int? PostorId = null
    ) : IQuery<IReadOnlyList<SubastaListDto>>;
}
