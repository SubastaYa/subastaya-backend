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
    public class BidsController : ControllerBase
    {
        private readonly ICommandHandler<CrearPujaCommand, int> _crearPujaHandler;

        public BidsController(ICommandHandler<CrearPujaCommand, int> crearPujaHandler)
        {
            _crearPujaHandler = crearPujaHandler;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceBid(int auctionId, [FromBody] BidRequestDto request, CancellationToken ct)
        {
            var buyerId = User.GetUserId();
            var nuevaPujaId = await _crearPujaHandler.HandleAsync(new CrearPujaCommand(auctionId, buyerId, request.Amount), ct);

            return Created($"api/subastas/{auctionId}/pujas/{nuevaPujaId}", new
            {
                id = nuevaPujaId,
                subastaId = auctionId,
                mensaje = "Oferta validada exitosamente."
            });
        }
    }
}
