using Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using System.Security.Claims;

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
                    s.UrlImagen,
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
                subasta.UrlImagen,
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Crear([FromBody] CrearSubastaDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var vendedorId))
            {
                return Unauthorized(new { mensaje = "No se pudo identificar al usuario autenticado." });
            }

            if (string.IsNullOrWhiteSpace(dto.Titulo))
            {
                return BadRequest(new { mensaje = "El título es obligatorio." });
            }

            if (string.IsNullOrWhiteSpace(dto.UrlImagen))
            {
                return BadRequest(new { mensaje = "La URL de la imagen es obligatoria." });
            }

            if (dto.PrecioBase <= 0)
            {
                return BadRequest(new { mensaje = "El precio base debe ser un valor positivo mayor a cero." });
            }

            if (dto.IncrementoMinimo <= 0)
            {
                return BadRequest(new { mensaje = "El incremento mínimo debe ser un valor positivo mayor a cero." });
            }

            if (dto.FechaFin <= dto.FechaInicio)
            {
                return BadRequest(new { mensaje = "La fecha de fin debe ser posterior a la fecha de inicio." });
            }

            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
            if (!categoriaExiste)
            {
                return BadRequest(new { mensaje = $"La categoría con ID {dto.CategoriaId} no existe." });
            }

            var ahora = DateTime.UtcNow;
            var estadoInicial = dto.FechaInicio <= ahora ? EstadoSubasta.Activa : EstadoSubasta.Programada;

            var subasta = new Subasta(
                vendedorId,
                dto.CategoriaId,
                dto.Titulo,
                dto.Descripcion,
                dto.UrlImagen,
                dto.PrecioBase,
                dto.IncrementoMinimo,
                dto.FechaInicio,
                dto.FechaFin,
                estadoInicial
            );

            _context.Subastas.Add(subasta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = subasta.Id },
                new { id = subasta.Id, mensaje = "Subasta creada exitosamente." }
            );
        }
    }
}
