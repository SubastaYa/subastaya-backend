using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.Ofertas.CrearOferta;
using Application.UseCases.Ofertas.ObtenerOfertaPorId;
using Application.UseCases.Ofertas.ObtenerOfertasPorSubastaId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Extensions;

namespace Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/subastas/{auctionId:int}/ofertas")]
    [Route("api/subastas/{auctionId:int}/ofertas")]
    [Route("api/v1/subastas/{auctionId:int}/bids")]
    [Route("api/subastas/{auctionId:int}/bids")]
    [Route("api/v1/auctions/{auctionId:int}/bids")]
    [Route("api/auctions/{auctionId:int}/bids")]
    public class OfertasController : ControllerBase
    {
        private readonly ICommandHandler<CrearOfertaCommand, int> _crearOfertaHandler;
        private readonly IQueryHandler<ObtenerOfertaPorIdQuery, OfertaResumenDto?> _detalleOfertaHandler;
        private readonly IQueryHandler<ObtenerOfertasPorSubastaIdQuery, IReadOnlyList<OfertaResumenDto>> _ofertasHandler;

        public OfertasController(
            ICommandHandler<CrearOfertaCommand, int> crearOfertaHandler,
            IQueryHandler<ObtenerOfertaPorIdQuery, OfertaResumenDto?> detalleOfertaHandler,
            IQueryHandler<ObtenerOfertasPorSubastaIdQuery, IReadOnlyList<OfertaResumenDto>> ofertasHandler)
        {
            _crearOfertaHandler = crearOfertaHandler;
            _detalleOfertaHandler = detalleOfertaHandler;
            _ofertasHandler = ofertasHandler;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(int auctionId, CancellationToken ct)
        {
            var ofertas = await _ofertasHandler.HandleAsync(new ObtenerOfertasPorSubastaIdQuery(auctionId), ct);
            return Ok(ofertas);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int auctionId, int id, CancellationToken ct)
        {
            var oferta = await _detalleOfertaHandler.HandleAsync(new ObtenerOfertaPorIdQuery(id), ct)
                ?? throw new KeyNotFoundException($"La oferta con ID {id} no existe.");

            return Ok(oferta);
        }

        [HttpPost]
        public async Task<IActionResult> CrearOferta(int auctionId, [FromBody] BidRequestDto request, CancellationToken ct)
        {
            // Obtenemos el ID del usuario comprador a partir del token JWT
            var compradorId = User.GetUserId();
            var nuevaOfertaId = await _crearOfertaHandler.HandleAsync(new CrearOfertaCommand(auctionId, compradorId, request.Amount), ct);

            // Devolvemos 201 Created con encabezado Location canónico
            return CreatedAtAction(
                nameof(GetById),
                new { auctionId, id = nuevaOfertaId },
                new
                {
                    id = nuevaOfertaId,
                    subastaId = auctionId,
                    mensaje = "Oferta validada exitosamente."
                }
            );
        }
    }
}
