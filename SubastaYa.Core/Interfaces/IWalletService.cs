using System.Threading.Tasks;
using SubastaYa.Core.DTOs;

namespace SubastaYa.Core.Interfaces
{
    public interface IWalletService
    {
        Task<WalletResponseDto> GetBalanceAsync(int userId);
        Task<WalletResponseDto> DepositAsync(int userId, decimal amount);
    }
}
