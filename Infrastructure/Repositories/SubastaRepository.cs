using Application.Common.Helpers;
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
                    s.FechaInicio,
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

            var postorLiderId = subasta.Pujas
                .OrderByDescending(p => p.Monto)
                .FirstOrDefault()?.CompradorId;

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
                    .Select(p => new OfertaResumenDto(
                        p.Id,
                        p.Monto,
                        p.FechaPuja,
                        UsuarioHelper.OfuscarNombre(p.Comprador.Nombre)
                    ))
                    .ToList(),
                postorLiderId
            );
        }

        public async Task<Subasta?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Subastas.FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<Subasta?> ObtenerConOfertasPorIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Subastas
                .Include(s => s.Pujas)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<IReadOnlyList<Subasta>> ObtenerVencidasParaLiquidacionAsync(DateTime ahora, CancellationToken ct = default)
        {
            return await _context.Subastas
                .Include(s => s.Pujas)
                    .ThenInclude(p => p.Comprador)
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

        public async Task<IReadOnlyList<MiOfertaSubastaDto>> ObtenerMisOfertasAsync(int postorId, CancellationToken ct = default)
        {
            var subastas = await _context.Subastas
                .AsNoTracking()
                .Include(s => s.Categoria)
                .Include(s => s.Pujas)
                .Where(s => s.Pujas.Any(p => p.CompradorId == postorId))
                .OrderByDescending(s => s.FechaFin)
                .ToListAsync(ct);

            var resultado = new List<MiOfertaSubastaDto>();
            foreach (var s in subastas)
            {
                var ofertaMaxima = s.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();
                var miOfertaMaxima = s.Pujas.Where(p => p.CompradorId == postorId).Max(p => p.Monto);
                var esGanador = s.Estado == EstadoSubasta.Finalizada && ofertaMaxima != null && ofertaMaxima.CompradorId == postorId;
                var esLider = s.Estado == EstadoSubasta.Activa && ofertaMaxima != null && ofertaMaxima.CompradorId == postorId;
                var precioActual = ofertaMaxima?.Monto ?? s.PrecioBase;

                resultado.Add(new MiOfertaSubastaDto(
                    s.Id,
                    s.Titulo,
                    s.UrlImagen,
                    s.Estado,
                    s.FechaInicio,
                    s.FechaFin,
                    s.PrecioBase,
                    precioActual,
                    miOfertaMaxima,
                    esGanador,
                    esLider,
                    s.Categoria.Nombre
                ));
            }

            return resultado;
        }

        public async Task<IReadOnlyList<MiPublicacionDto>> ObtenerMisPublicacionesAsync(int vendedorId, CancellationToken ct = default)
        {
            var subastas = await _context.Subastas
                .AsNoTracking()
                .Include(s => s.Categoria)
                .Include(s => s.Pujas)
                    .ThenInclude(p => p.Comprador)
                .Where(s => s.VendedorId == vendedorId)
                .OrderByDescending(s => s.FechaInicio)
                .ToListAsync(ct);

            var resultado = new List<MiPublicacionDto>();
            foreach (var s in subastas)
            {
                var pujaGanadora = s.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();
                var totalPujas = s.Pujas.Count;
                var precioActual = pujaGanadora?.Monto ?? s.PrecioBase;
                var montoRecaudado = (s.Estado == EstadoSubasta.Finalizada && pujaGanadora != null) ? pujaGanadora.Monto : 0m;
                var ganadorNombre = (s.Estado == EstadoSubasta.Finalizada && pujaGanadora != null)
                    ? UsuarioHelper.OfuscarNombre(pujaGanadora.Comprador?.Nombre)
                    : null;

                resultado.Add(new MiPublicacionDto(
                    s.Id,
                    s.Titulo,
                    s.UrlImagen,
                    s.PrecioBase,
                    precioActual,
                    s.Estado,
                    s.FechaInicio,
                    s.FechaFin,
                    s.Categoria.Nombre,
                    totalPujas,
                    montoRecaudado,
                    ganadorNombre
                ));
            }

            return resultado;
        }
    }
}
