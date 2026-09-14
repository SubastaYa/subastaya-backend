using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.Billetera.ObtenerMovimientos
{
    public record ObtenerMovimientosQuery(int UsuarioId) : IQuery<IReadOnlyList<TransaccionLedgerDto>>;
}
