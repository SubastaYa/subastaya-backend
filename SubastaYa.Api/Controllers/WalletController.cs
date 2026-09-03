using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubastaYa.Core.DTOs;
using SubastaYa.Core.Interfaces;

namespace SubastaYa.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userId = GetUserId();
            var balance = await _walletService.GetBalanceAsync(userId);
            return Ok(balance);
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDto request)
        {
            if (request == null || request.Amount <= 0)
            {
                return BadRequest(new { mensaje = "El monto a depositar debe ser mayor a cero." });
            }

            var userId = GetUserId();
            var balance = await _walletService.DepositAsync(userId, request.Amount);
            return Ok(balance);
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
