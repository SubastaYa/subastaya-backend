using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.Subastas.Commands.CrearSubasta;
using Application.UseCases.Subastas.Queries.ObtenerCatalogo;
using Application.UseCases.Subastas.Queries.ObtenerDetalle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SubastaYa.Api.Controllers
{
    [Route("api/subastas")]
    [Route("api/auctions")]
    [ApiController]
    public class SubastasController : ControllerBase
    {
        private readonly ICommandHandler<CrearSubastaCommand, int> _crearHandler;
        private readonly IQueryHandler<ObtenerCatalogoQuery, IReadOnlyList<SubastaListDto>> _catalogoHandler;
        private readonly IQueryHandler<ObtenerSubastaPorIdQuery, SubastaDetalleDto?> _detalleHandler;

        public SubastasController(
            ICommandHandler<CrearSubastaCommand, int> crearHandler,
            IQueryHandler<ObtenerCatalogoQuery, IReadOnlyList<SubastaListDto>> catalogoHandler,
            IQueryHandler<ObtenerSubastaPorIdQuery, SubastaDetalleDto?> detalleHandler)
        {
            _crearHandler = crearHandler;
            _catalogoHandler = catalogoHandler;
            _detalleHandler = detalleHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ObtenerCatalogoQuery query, CancellationToken ct)
        {
            var resultado = await _catalogoHandler.HandleAsync(query, ct);
            return Ok(resultado);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var subasta = await _detalleHandler.HandleAsync(new ObtenerSubastaPorIdQuery(id), ct);
            if (subasta is null)
            {
                return NotFound(new { mensaje = $"La subasta con ID {id} no existe." });
            }

            return Ok(subasta);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] CrearSubastaCommand command, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var vendedorId))
            {
                return Unauthorized(new { mensaje = "No se pudo identificar al usuario autenticado." });
            }
            command.VendedorId = vendedorId;
            var nuevoId = await _crearHandler.HandleAsync(command, ct);
            return CreatedAtAction(
                nameof(GetById),
                new { id = nuevoId },
                new { id = nuevoId, mensaje = "Subasta creada exitosamente." }
            );
        }
    }
}
