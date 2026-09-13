using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.Pujas.CrearPuja;
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

        public PujasController(ICommandHandler<CrearPujaCommand, int> crearPujaHandler)
        {
            _crearPujaHandler = crearPujaHandler;
        }

        [HttpPost]
        public async Task<IActionResult> CrearPuja(int auctionId, [FromBody] BidRequestDto request, CancellationToken ct)
        {
            // Obtenemos el ID del usuario autenticado a partir del token JWT
            var compradorId = User.GetUserId();
            var nuevaPujaId = await _crearPujaHandler.HandleAsync(new CrearPujaCommand(auctionId, compradorId, request.Amount), ct);

            // Devolvemos 201 Created con la URI de la nueva puja
            return Created($"/api/subastas/{auctionId}/pujas/{nuevaPujaId}", new
            {
                id = nuevaPujaId,
                subastaId = auctionId,
                mensaje = "Oferta validada exitosamente."
            });
        }
    }
}
