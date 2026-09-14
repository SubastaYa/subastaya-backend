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
            decimal? precioMin = null,
            decimal? precioMax = null,
            string? orden = null,
            int? page = null,
            int? pageSize = null,
            int? vendedorId = null,
            int? postorId = null,
            CancellationToken ct = default)
        {
            var query = _context.Subastas
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoSubasta>(estado, true, out var estadoEnum))
            {
                query = query.Where(s => s.Estado == estadoEnum);
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(s => s.CategoriaId == categoriaId.Value);
            }

            if (vendedorId.HasValue)
            {
                query = query.Where(s => s.VendedorId == vendedorId.Value);
            }

            if (postorId.HasValue)
            {
                query = query.Where(s => s.Pujas.Any(p => p.CompradorId == postorId.Value));
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(s => s.Titulo.Contains(busqueda) || s.Descripcion.Contains(busqueda));
            }

            if (precioMin.HasValue)
            {
                query = query.Where(s => s.PrecioBase >= precioMin.Value);
            }

            if (precioMax.HasValue)
            {
                query = query.Where(s => s.PrecioBase <= precioMax.Value);
            }

            query = (orden?.ToLowerInvariant()) switch
            {
                "precio_asc" => query.OrderBy(s => s.PrecioBase),
                "mayor_puja" or "precio_desc" => query.OrderByDescending(s => s.Pujas.Select(p => (decimal?)p.Monto).Max() ?? s.PrecioBase),
                "tiempo_restante" or "fin_asc" or "proximas" => query.OrderBy(s => s.FechaFin),
                "fin_desc" => query.OrderByDescending(s => s.FechaFin),
                _ => query.OrderByDescending(s => s.FechaInicio)
            };

            // Paginación defensiva obligatoria: página por defecto 1, tamaño acotado entre 1 y 50
            var paginaActual = (page.HasValue && page.Value > 0) ? page.Value : 1;
            var tamanoPagina = Math.Clamp(pageSize ?? 10, 1, 50);
            var skip = (paginaActual - 1) * tamanoPagina;
            query = query.Skip(skip).Take(tamanoPagina);

            return await query
                .Select(s => new SubastaListDto(
                    s.Id,
                    s.Titulo,
                    s.UrlImagen,
                    s.PrecioBase,
                    s.Pujas.Select(p => (decimal?)p.Monto).Max() ?? s.PrecioBase,
                    s.Estado,
                    s.FechaFin,
                    s.Categoria.Nombre,
                    s.Vendedor.Nombre,
                    s.Pujas.Count
                ))
                .ToListAsync(ct);
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

        public async Task<IReadOnlyList<Subasta>> ObtenerVencidasParaLiquidacionAsync(DateTime ahora, CancellationToken ct = default)
        {
            return await _context.Subastas
                .Include(s => s.Pujas)
                .Where(s => s.Estado == EstadoSubasta.Activa && s.FechaFin <= ahora)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Subasta>> ObtenerProgramadasParaIniciarAsync(DateTime ahora, CancellationToken ct = default)
        {
            return await _context.Subastas
                .Where(s => s.Estado == EstadoSubasta.Programada && s.FechaInicio <= ahora)
                .ToListAsync(ct);
        }

        public async Task AgregarAsync(Subasta subasta, CancellationToken ct = default)
        {
            await _context.Subastas.AddAsync(subasta, ct);
        }

        public void Actualizar(Subasta subasta)
        {
            _context.Subastas.Update(subasta);
        }

        public async Task<bool> CategoriaExisteAsync(int id, CancellationToken ct = default)
        {
            return await _context.Categorias.AnyAsync(c => c.Id == id, ct);
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
