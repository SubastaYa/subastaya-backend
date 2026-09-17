using Application.DTOs;

namespace Application.UseCases.Billetera.ObtenerBalance
{
    public class ObtenerBalanceQueryHandler : IQueryHandler<ObtenerBalanceQuery, WalletResponseDto>
    {
        private readonly IBilleteraRepository _billeteraRepository;

        public ObtenerBalanceQueryHandler(IBilleteraRepository billeteraRepository)
        {
            _billeteraRepository = billeteraRepository;
        }

        public async Task<WalletResponseDto> HandleAsync(ObtenerBalanceQuery query, CancellationToken ct = default)
        {
            var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(query.UsuarioId, ct)
                ?? throw new KeyNotFoundException($"No se encontró la billetera para el usuario con ID {query.UsuarioId}.");

            return new WalletResponseDto(billetera.SaldoTotal, billetera.SaldoRetenido, billetera.SaldoDisponible);
        }
    }
}
