using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.Pujas.CrearPuja;
using Application.UseCases.Pujas.ObtenerPujaPorId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Extensions;

namespace Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/subastas/{auctionId:int}/pujas")]
    [Route("api/subastas/{auctionId:int}/bids")]
    [Route("api/auctions/{auctionId:int}/bids")]
    public class PujasController : ControllerBase
    {
        private readonly ICommandHandler<CrearPujaCommand, int> _crearPujaHandler;
        private readonly IQueryHandler<ObtenerPujaPorIdQuery, PujaResumenDto?> _detallePujaHandler;

        public PujasController(
            ICommandHandler<CrearPujaCommand, int> crearPujaHandler,
            IQueryHandler<ObtenerPujaPorIdQuery, PujaResumenDto?> detallePujaHandler)
        {
            _crearPujaHandler = crearPujaHandler;
            _detallePujaHandler = detallePujaHandler;
        }

        [HttpGet("{id:int}", Name = "GetPujaPorId")]
        public async Task<IActionResult> GetById(int auctionId, int id, CancellationToken ct)
        {
            var puja = await _detallePujaHandler.HandleAsync(new ObtenerPujaPorIdQuery(id), ct)
                ?? throw new KeyNotFoundException($"La oferta con ID {id} no existe.");

            return Ok(puja);
        }

        [HttpPost]
        public async Task<IActionResult> CrearPuja(int auctionId, [FromBody] BidRequestDto request, CancellationToken ct)
        {
            // Obtenemos el ID del usuario autenticado a partir del token JWT
            var compradorId = User.GetUserId();
            var nuevaPujaId = await _crearPujaHandler.HandleAsync(new CrearPujaCommand(auctionId, compradorId, request.Amount), ct);

            // Devolvemos 201 Created con encabezado Location absoluto canónico
            return CreatedAtRoute(
                "GetPujaPorId",
                new { auctionId, id = nuevaPujaId },
                new
                {
                    id = nuevaPujaId,
                    subastaId = auctionId,
                    mensaje = "Oferta validada exitosamente."
                }
            );
        }
    }
}
