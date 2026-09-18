using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BilleteraRepository : IBilleteraRepository
    {
        private readonly ApplicationDbContext _context;

        public BilleteraRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Billetera?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default)
        {
            return await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId, ct);
        }

        public async Task AgregarAsync(Billetera billetera, CancellationToken ct = default)
        {
            await _context.Billeteras.AddAsync(billetera, ct);
        }

        public async Task AgregarTransaccionLedgerAsync(TransaccionLedger transaccion, CancellationToken ct = default)
        {
            await _context.TransaccionesLedger.AddAsync(transaccion, ct);
        }

        public async Task<IReadOnlyList<TransaccionLedgerDto>> ObtenerMovimientosPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default)
        {
            return await _context.TransaccionesLedger
                .AsNoTracking()
                .Where(t => t.Billetera.UsuarioId == usuarioId)
                .OrderByDescending(t => t.Fecha)
                .Select(t => new TransaccionLedgerDto(
                    t.Id,
                    t.Tipo,
                    t.Monto,
                    t.Fecha,
                    t.SubastaId,
                    t.Subasta != null ? t.Subasta.Titulo : null
                ))
                .ToListAsync(ct);
        }
    }
}
