using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.Pujas.ObtenerPujasPorSubastaId
{
    public record ObtenerPujasPorSubastaIdQuery(int SubastaId) : IQuery<IReadOnlyList<PujaResumenDto>>;
}
