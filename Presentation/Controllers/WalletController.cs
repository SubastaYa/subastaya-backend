using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.Billetera.DepositarFondos;
using Application.UseCases.Billetera.ObtenerBalance;
using Application.UseCases.Billetera.ObtenerMovimientos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Extensions;

namespace Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/billetera")]
    [Route("api/v1/wallet")]
    [Route("api/billetera")]
    [Route("api/wallet")]
    public class WalletController : ControllerBase
    {
        private readonly IQueryHandler<ObtenerBalanceQuery, WalletResponseDto> _balanceHandler;
        private readonly ICommandHandler<DepositarFondosCommand, WalletResponseDto> _depositarHandler;
        private readonly IQueryHandler<ObtenerMovimientosQuery, IReadOnlyList<TransaccionLedgerDto>> _movimientosHandler;

        public WalletController(
            IQueryHandler<ObtenerBalanceQuery, WalletResponseDto> balanceHandler,
            ICommandHandler<DepositarFondosCommand, WalletResponseDto> depositarHandler,
            IQueryHandler<ObtenerMovimientosQuery, IReadOnlyList<TransaccionLedgerDto>> movimientosHandler)
        {
            _balanceHandler = balanceHandler;
            _depositarHandler = depositarHandler;
            _movimientosHandler = movimientosHandler;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance(CancellationToken ct)
        {
            var balance = await _balanceHandler.HandleAsync(new ObtenerBalanceQuery(User.GetUserId()), ct);
            return Ok(balance);
        }

        [HttpPost("deposito")]
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDto request, CancellationToken ct)
        {
            var balance = await _depositarHandler.HandleAsync(new DepositarFondosCommand(User.GetUserId(), request.Amount), ct);
            return Ok(balance);
        }

        // Historial de movimientos de la billetera (transacciones contables)
        [HttpGet("transactions")]
        [HttpGet("movimientos")]
        public async Task<IActionResult> GetTransactions(CancellationToken ct)
        {
            var movimientos = await _movimientosHandler.HandleAsync(new ObtenerMovimientosQuery(User.GetUserId()), ct);
            return Ok(movimientos);
        }
    }
}
