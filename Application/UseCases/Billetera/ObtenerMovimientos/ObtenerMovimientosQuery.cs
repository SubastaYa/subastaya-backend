using Application.DTOs;

namespace Application.UseCases.Billetera.ObtenerMovimientos
{
    public record ObtenerMovimientosQuery(int UsuarioId) : IQuery<IReadOnlyList<TransaccionLedgerDto>>;
}
