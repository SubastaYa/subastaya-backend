using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.Ofertas.ObtenerOfertaPorId
{
    public record ObtenerOfertaPorIdQuery(int Id) : IQuery<OfertaResumenDto?>;
}
