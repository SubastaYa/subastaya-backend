using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.Pujas.ObtenerPujaPorId
{
    public record ObtenerPujaPorIdQuery(int Id) : IQuery<PujaResumenDto?>;
}
