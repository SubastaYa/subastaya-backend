using Application.DTOs;

namespace Application.UseCases.Ofertas.ObtenerOfertaPorId
{
    public record ObtenerOfertaPorIdQuery(int Id) : IQuery<OfertaResumenDto?>;
}
