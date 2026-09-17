using Application.Common.Helpers;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class OfertaRepository : IOfertaRepository
    {
        private readonly ApplicationDbContext _context;

        public OfertaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AgregarAsync(Oferta oferta, CancellationToken ct = default)
        {
            await _context.Ofertas.AddAsync(oferta, ct);
        }

        public async Task<OfertaResumenDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            var oferta = await _context.Ofertas
                .AsNoTracking()
                .Include(p => p.Comprador)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (oferta is null)
            {
                return null;
            }

            return new OfertaResumenDto(
                oferta.Id,
                oferta.Monto,
                oferta.FechaOferta,
                UsuarioHelper.OfuscarNombre(oferta.Comprador?.Nombre)
            );
        }

        public async Task<IReadOnlyList<OfertaResumenDto>> ObtenerPorSubastaIdAsync(int subastaId, CancellationToken ct = default)
        {
            var ofertas = await _context.Ofertas
                .AsNoTracking()
                .Include(p => p.Comprador)
                .Where(p => p.SubastaId == subastaId)
                .OrderByDescending(p => p.FechaOferta)
                .ToListAsync(ct);

            return ofertas.Select(p => new OfertaResumenDto(
                p.Id,
                p.Monto,
                p.FechaOferta,
                UsuarioHelper.OfuscarNombre(p.Comprador?.Nombre)
            )).ToList();
        }
    }
}
