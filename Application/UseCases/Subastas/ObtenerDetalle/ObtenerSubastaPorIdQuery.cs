using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.Subastas.ObtenerDetalle
{
    public record ObtenerSubastaPorIdQuery(int Id) : IQuery<SubastaDetalleDto?>;
}
