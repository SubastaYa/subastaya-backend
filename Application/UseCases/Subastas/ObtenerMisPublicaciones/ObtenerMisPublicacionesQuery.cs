using Application.DTOs;

namespace Application.UseCases.Subastas.ObtenerMisPublicaciones
{
    public record ObtenerMisPublicacionesQuery(int VendedorId) : IQuery<IReadOnlyList<MiPublicacionDto>>;
}
