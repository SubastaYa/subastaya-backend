using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Core.DTOs;
using SubastaYa.Infrastructure.Data;
using SubastaYa.Core.Enums;

namespace SubastaYa.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubastasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SubastasController(ApplicationDbContext context)
        {
            _context = context;
        }
                
        [HttpGet]
        public async Task<IActionResult> ObtenerCatalogo(
            [FromQuery] int? categoriaId,
            [FromQuery] string? estado,
            [FromQuery] string? busqueda)
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
            else
            {
                query = query.Where(s => s.Estado == EstadoSubasta.Activa);
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(s => s.CategoriaId == categoriaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(s => s.Titulo.Contains(busqueda) || s.Descripcion.Contains(busqueda));
            }

            var resultado = await query
                .OrderByDescending(s => s.FechaInicio)
                .Select(s => new SubastaListDto(
                    s.Id,
                    s.Titulo,
                    s.PrecioBase,
                    s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase,
                    s.Estado,
                    s.FechaFin,
                    s.Categoria.Nombre,
                    s.Vendedor.Nombre,
                    s.Pujas.Count
                ))
                .ToListAsync();

            return Ok(resultado);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var subasta = await _context.Subastas
                .AsNoTracking()
                .Include(s => s.Categoria)
                .Include(s => s.Vendedor)
                .Include(s => s.Pujas)
                    .ThenInclude(p => p.Comprador)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subasta is null)
            {
                return NotFound(new { mensaje = $"La subasta con ID {id} no existe." });
            }

            var detalleDto = new SubastaDetalleDto(
                subasta.Id,
                subasta.Titulo,
                subasta.Descripcion,
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
                        p.Comprador.Nombre
                    ))
                    .ToList()
            );

            return Ok(detalleDto);
        }
    }
}
