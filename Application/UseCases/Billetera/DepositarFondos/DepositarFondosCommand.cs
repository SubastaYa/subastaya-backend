using Application.DTOs;

namespace Application.UseCases.Billetera.DepositarFondos
{
    public record DepositarFondosCommand(int UsuarioId, decimal Monto) : ICommand<WalletResponseDto>;
}
