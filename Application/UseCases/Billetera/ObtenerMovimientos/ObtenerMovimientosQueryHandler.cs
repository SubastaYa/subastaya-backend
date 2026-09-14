using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Persistence;

namespace Application.UseCases.Billetera.ObtenerMovimientos
{
    public class ObtenerMovimientosQueryHandler : IQueryHandler<ObtenerMovimientosQuery, IReadOnlyList<TransaccionLedgerDto>>
    {
        private readonly IBilleteraRepository _billeteraRepository;

        public ObtenerMovimientosQueryHandler(IBilleteraRepository billeteraRepository)
        {
            _billeteraRepository = billeteraRepository;
        }

        public async Task<IReadOnlyList<TransaccionLedgerDto>> HandleAsync(ObtenerMovimientosQuery query, CancellationToken ct = default)
        {
            return await _billeteraRepository.ObtenerMovimientosPorUsuarioIdAsync(query.UsuarioId, ct);
        }
    }
}
