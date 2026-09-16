using Application.Common.Helpers;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PujaRepository : IPujaRepository
    {
        private readonly ApplicationDbContext _context;

        public PujaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AgregarAsync(Puja puja, CancellationToken ct = default)
        {
            await _context.Pujas.AddAsync(puja, ct);
        }

        public async Task<Puja?> ObtenerUltimaPujaAsync(int subastaId, CancellationToken ct = default)
        {
            return await _context.Pujas
                .AsNoTracking()
                .Where(p => p.SubastaId == subastaId)
                .OrderByDescending(p => p.Monto)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<PujaResumenDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            var puja = await _context.Pujas
                .AsNoTracking()
                .Include(p => p.Comprador)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (puja is null)
            {
                return null;
            }

            return new PujaResumenDto(
                puja.Id,
                puja.Monto,
                puja.FechaPuja,
                UsuarioHelper.OfuscarNombre(puja.Comprador?.Nombre)
            );
        }

        public async Task<IReadOnlyList<PujaResumenDto>> ObtenerPorSubastaIdAsync(int subastaId, CancellationToken ct = default)
        {
            var pujas = await _context.Pujas
                .AsNoTracking()
                .Include(p => p.Comprador)
                .Where(p => p.SubastaId == subastaId)
                .OrderByDescending(p => p.FechaPuja)
                .ToListAsync(ct);

            return pujas.Select(p => new PujaResumenDto(
                p.Id,
                p.Monto,
                p.FechaPuja,
                UsuarioHelper.OfuscarNombre(p.Comprador?.Nombre)
            )).ToList();
        }
    }
}
