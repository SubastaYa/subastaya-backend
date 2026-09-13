using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IWalletService
    {
        Task<WalletResponseDto> GetBalanceAsync(int userId, CancellationToken ct = default);
        Task<WalletResponseDto> DepositAsync(int userId, decimal amount, CancellationToken ct = default);
        Task<IReadOnlyList<TransaccionLedgerDto>> ObtenerMovimientosPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default);
    }
}
