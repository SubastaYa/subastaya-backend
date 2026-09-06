using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
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

        public async Task AgregarTransaccionLedgerAsync(TransaccionLedger transaccion, CancellationToken ct = default)
        {
            await _context.TransaccionesLedger.AddAsync(transaccion, ct);
        }
    }
}
