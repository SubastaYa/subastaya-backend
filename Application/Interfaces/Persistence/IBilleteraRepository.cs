using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IBilleteraRepository
    {
        Task<Billetera?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default);
        Task AgregarTransaccionLedgerAsync(TransaccionLedger transaccion, CancellationToken ct = default);
        Task<IReadOnlyList<TransaccionLedgerDto>> ObtenerMovimientosPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default);
    }
}
