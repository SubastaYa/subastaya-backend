using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SubastaRepository : ISubastaRepository
    {
        private readonly ApplicationDbContext _context;

        public SubastaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<SubastaListDto>> ObtenerCatalogoAsync(
            int? categoriaId,
            string? estado,
            string? busqueda,
            string? orden = null,
            int? page = null,
            int? pageSize = null,
            CancellationToken ct = default)
        {
            var query = _context.Subastas
                .AsNoTracking()
                .Include(s => s.Categoria)
                .Include(s => s.Vendedor)
                .Include(s => s.Pujas)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoSubasta>(estado, true, out var estadoEnum))
            {
                query = query.Where(s => s.Estado == estadoEnum);
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(s => s.CategoriaId == categoriaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(s => s.Titulo.Contains(busqueda) || s.Descripcion.Contains(busqueda));
            }

            query = (orden?.ToLowerInvariant()) switch
            {
                "precio_asc" => query.OrderBy(s => s.PrecioBase),
                "precio_desc" => query.OrderByDescending(s => s.PrecioBase),
                "fin_asc" or "proximas" => query.OrderBy(s => s.FechaFin),
                "fin_desc" => query.OrderByDescending(s => s.FechaFin),
                _ => query.OrderByDescending(s => s.FechaInicio)
            };

            if (page.HasValue && page.Value > 0)
            {
                var take = pageSize.HasValue && pageSize.Value > 0 ? Math.Min(pageSize.Value, 50) : 10;
                var skip = (page.Value - 1) * take;
                query = query.Skip(skip).Take(take);
            }

            var items = await query
                .Select(s => new SubastaListDto(
                    s.Id,
                    s.Titulo,
                    s.UrlImagen,
                    s.PrecioBase,
                    s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase,
                    s.Estado,
                    s.FechaFin,
                    s.Categoria.Nombre,
                    s.Vendedor.Nombre,
                    s.Pujas.Count
                ))
                .ToListAsync(ct);

            return items;
        }

        public async Task<SubastaDetalleDto?> ObtenerDetallePorIdAsync(int id, CancellationToken ct = default)
        {
            var subasta = await _context.Subastas
                .AsNoTracking()
                .Include(s => s.Categoria)
                .Include(s => s.Vendedor)
                .Include(s => s.Pujas)
                    .ThenInclude(p => p.Comprador)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            if (subasta is null)
            {
                return null;
            }

            return new SubastaDetalleDto(
                subasta.Id,
                subasta.Titulo,
                subasta.Descripcion,
                subasta.UrlImagen,
                subasta.PrecioBase,
                subasta.Pujas.Any() ? subasta.Pujas.Max(p => p.Monto) : subasta.PrecioBase,
                subasta.IncrementoMinimo,
                subasta.Estado,
                subasta.FechaInicio,
                subasta.FechaFin,
                subasta.CategoriaId,
                subasta.Categoria.Nombre,
                subasta.VendedorId,
                subasta.Vendedor.Nombre,
                subasta.Pujas
                    .OrderByDescending(p => p.FechaPuja)
                    .Take(5)
                    .Select(p => new PujaResumenDto(
                        p.Id,
                        p.Monto,
                        p.FechaPuja,
                        OfuscarNombre(p.Comprador.Nombre)
                    ))
                    .ToList()
            );
        }

        public async Task<Subasta?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Subastas.FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<Subasta?> ObtenerConPujasPorIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Subastas
                .Include(s => s.Pujas)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task AgregarAsync(Subasta subasta, CancellationToken ct = default)
        {
            await _context.Subastas.AddAsync(subasta, ct);
        }

        public void Actualizar(Subasta subasta)
        {
            _context.Entry(subasta).Property(s => s.Estado).IsModified = true;
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
