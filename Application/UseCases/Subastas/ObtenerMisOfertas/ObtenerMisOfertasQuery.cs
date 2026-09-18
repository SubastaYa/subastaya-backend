using Application.DTOs;

namespace Application.UseCases.Subastas.ObtenerMisOfertas
{
    public record ObtenerMisOfertasQuery(int PostorId) : IQuery<IReadOnlyList<MiOfertaSubastaDto>>;
}
