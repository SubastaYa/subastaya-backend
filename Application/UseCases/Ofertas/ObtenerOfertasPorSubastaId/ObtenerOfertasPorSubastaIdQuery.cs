using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.Ofertas.ObtenerOfertasPorSubastaId
{
    public record ObtenerOfertasPorSubastaIdQuery(int SubastaId) : IQuery<IReadOnlyList<OfertaResumenDto>>;
}
