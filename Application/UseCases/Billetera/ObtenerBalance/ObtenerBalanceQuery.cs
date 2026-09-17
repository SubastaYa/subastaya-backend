using Application.DTOs;

namespace Application.UseCases.Billetera.ObtenerBalance
{
    public record ObtenerBalanceQuery(int UsuarioId) : IQuery<WalletResponseDto>;
}
