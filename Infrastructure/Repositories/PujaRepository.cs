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
                OfuscarNombre(puja.Comprador?.Nombre)
            );
        }

        private static string OfuscarNombre(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return "Anónimo";

            var trimmed = nombre.Trim();
            if (trimmed.Length <= 2)
                return $"{trimmed[0]}***";

            return $"{trimmed[0]}***{trimmed[^1]}";
        }
    }
}
