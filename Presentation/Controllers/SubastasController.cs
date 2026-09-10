using Application.DTOs;
using Application.UseCases.Subastas.CrearSubasta;
using Application.UseCases.Subastas.ObtenerCatalogo;
using Application.UseCases.Subastas.ObtenerDetalle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Extensions;

namespace Presentation.Controllers
{
    [Route("api/subastas")]
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
            var subasta = await _detalleHandler.HandleAsync(new ObtenerSubastaPorIdQuery(id), ct)
                ?? throw new KeyNotFoundException($"La subasta con ID {id} no existe.");

            return Ok(subasta);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CrearSubasta([FromBody] CrearSubastaRequestDto request, CancellationToken ct)
        {
            var command = new CrearSubastaCommand(
                User.GetUserId(),
                request.CategoriaId,
                request.Titulo,
                request.Descripcion,
                request.UrlImagen,
                request.PrecioBase,
                request.IncrementoMinimo,
                request.FechaInicio,
                request.FechaFin
            );

            var nuevoId = await _crearHandler.HandleAsync(command, ct);
            return CreatedAtAction(
                nameof(GetById),
                new { id = nuevoId },
                new { id = nuevoId, mensaje = "Subasta creada exitosamente." }
            );
        }
    }
}
