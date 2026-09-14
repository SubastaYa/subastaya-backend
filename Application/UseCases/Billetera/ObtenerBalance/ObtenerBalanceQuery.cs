using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases.Billetera.ObtenerBalance
{
    public record ObtenerBalanceQuery(int UsuarioId) : IQuery<WalletResponseDto>;
}
