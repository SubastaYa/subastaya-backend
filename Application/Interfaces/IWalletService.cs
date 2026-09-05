using Application.DTOs;

namespace Application.Interfaces
{
    public interface IWalletService
    {
        Task<WalletResponseDto> GetBalanceAsync(int userId);
        Task<WalletResponseDto> DepositAsync(int userId, decimal amount);
    }
}
