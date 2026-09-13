using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Extensions;

namespace Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/billetera")]
    [Route("api/wallet")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance(CancellationToken ct)
        {
            var balance = await _walletService.GetBalanceAsync(User.GetUserId(), ct);
            return Ok(balance);
        }

        [HttpPost("deposito")]
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDto request, CancellationToken ct)
        {
            var balance = await _walletService.DepositAsync(User.GetUserId(), request.Amount, ct);
            return Ok(balance);
        }

        // Historial de movimientos de la billetera (transacciones contables)
        [HttpGet("transactions")]
        [HttpGet("movimientos")]
        public async Task<IActionResult> GetTransactions(CancellationToken ct)
        {
            var movimientos = await _walletService.ObtenerMovimientosPorUsuarioIdAsync(User.GetUserId(), ct);
            return Ok(movimientos);
        }
    }
}
