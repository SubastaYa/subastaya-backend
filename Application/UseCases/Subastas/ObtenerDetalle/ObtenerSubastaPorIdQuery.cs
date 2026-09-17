using Application.DTOs;

namespace Application.UseCases.Subastas.ObtenerDetalle
{
    public record ObtenerSubastaPorIdQuery(int Id) : IQuery<SubastaDetalleDto?>;
}
