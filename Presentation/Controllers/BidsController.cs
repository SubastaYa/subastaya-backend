using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;

namespace SubastaYa.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/auctions/{auctionId:int}/bids")]
    [Route("api/subastas/{auctionId:int}/bids")]
    public class BidsController : ControllerBase
    {
        private readonly IBidService _bidService;

        public BidsController(IBidService bidService)
        {
            _bidService = bidService;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceBid(int auctionId, [FromBody] BidRequestDto request)
        {
            if (request == null || request.Amount <= 0)
            {
                return BadRequest(new { mensaje = "El monto de la puja debe ser mayor a cero." });
            }

            var buyerId = GetUserId();

            try
            {
                await _bidService.PlaceBidAsync(auctionId, buyerId, request.Amount);
                return Ok(new { mensaje = "Oferta validada exitosamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (SaldoInsuficienteException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        private int GetUserId()
        {
            var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(claimValue) || !int.TryParse(claimValue, out var userId))
            {
                throw new UnauthorizedAccessException("Identificador de usuario no válido o ausente en el token de autenticación.");
            }

            return userId;
        }
    }
}
