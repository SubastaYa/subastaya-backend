using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.UseCases.Subastas.CrearSubasta;
using Application.UseCases.Subastas.ObtenerCatalogo;
using Application.UseCases.Subastas.ObtenerDetalle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Extensions;

namespace Presentation.Controllers
{
    [Route("api/v1/subastas")]
    [Route("api/v1/auctions")]
    [Route("api/subastas")]
    [Route("api/auctions")]
    [ApiController]
    public class SubastasController : ControllerBase
    {
        private readonly ICommandHandler<CrearSubastaCommand, int> _crearHandler;
        private readonly IQueryHandler<ObtenerCatalogoQuery, IReadOnlyList<SubastaListDto>> _catalogoHandler;
        private readonly IQueryHandler<ObtenerSubastaPorIdQuery, SubastaDetalleDto?> _detalleHandler;
        private readonly ISubastaRepository _subastaRepository;

        public SubastasController(
            ICommandHandler<CrearSubastaCommand, int> crearHandler,
            IQueryHandler<ObtenerCatalogoQuery, IReadOnlyList<SubastaListDto>> catalogoHandler,
            IQueryHandler<ObtenerSubastaPorIdQuery, SubastaDetalleDto?> detalleHandler,
            ISubastaRepository subastaRepository)
        {
            _crearHandler = crearHandler;
            _catalogoHandler = catalogoHandler;
            _detalleHandler = detalleHandler;
            _subastaRepository = subastaRepository;
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

        [Authorize]
        [HttpGet("mis-publicaciones")]
        [HttpGet("my-auctions")]
        [HttpGet("/api/v1/users/me/auctions")]
        [HttpGet("/api/users/me/auctions")]
        public async Task<IActionResult> GetMisPublicaciones(CancellationToken ct)
        {
            var resultado = await _subastaRepository.ObtenerMisPublicacionesAsync(User.GetUserId(), ct);
            return Ok(resultado);
        }

        [Authorize]
        [HttpGet("mis-pujas")]
        [HttpGet("my-bids")]
        [HttpGet("/api/v1/users/me/bids")]
        [HttpGet("/api/users/me/bids")]
        public async Task<IActionResult> GetMisPujas(CancellationToken ct)
        {
            var resultado = await _subastaRepository.ObtenerMisPujasAsync(User.GetUserId(), ct);
            return Ok(resultado);
        }
    }
}
