using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Extensions;

namespace Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/billetera")]
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
            var balance = await _walletService.GetBalanceAsync(User.GetUserId());
            return Ok(balance);
        }

        [HttpPost("deposito")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDto request)
        {
            var balance = await _walletService.DepositAsync(User.GetUserId(), request.Amount);
            return Ok(balance);
        }
    }
}
