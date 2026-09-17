using Application.DTOs;

namespace Application.UseCases.Ofertas.ObtenerOfertasPorSubastaId
{
    public record ObtenerOfertasPorSubastaIdQuery(int SubastaId) : IQuery<IReadOnlyList<OfertaResumenDto>>;
}
