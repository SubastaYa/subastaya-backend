using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.Billetera.DepositarFondos
{
    public record DepositarFondosCommand(int UsuarioId, decimal Monto) : ICommand<WalletResponseDto>;
}
