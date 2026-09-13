using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Application.Interfaces.Services;
using Application.Interfaces.Persistence;

namespace Infrastructure.Services
{
    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBilleteraRepository _billeteraRepository;

        public WalletService(ApplicationDbContext context, IBilleteraRepository billeteraRepository)
        {
            _context = context;
            _billeteraRepository = billeteraRepository;
        }

        public async Task<WalletResponseDto> GetBalanceAsync(int userId, CancellationToken ct = default)
        {
            var billetera = await _context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == userId, ct)
                ?? throw new KeyNotFoundException($"No se encontró la billetera para el usuario con ID {userId}.");

            return new WalletResponseDto(billetera.SaldoTotal, billetera.SaldoRetenido, billetera.SaldoDisponible);
        }

        public async Task<WalletResponseDto> DepositAsync(int userId, decimal amount, CancellationToken ct = default)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("El monto a depositar debe ser mayor a cero.", nameof(amount));
            }

            var billetera = await _context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == userId, ct)
                ?? throw new KeyNotFoundException($"No se encontró la billetera para el usuario con ID {userId}.");

            billetera.Depositar(amount);

            var transaccion = new TransaccionLedger(billetera.Id, TipoTransaccion.Deposito, amount);
            _context.TransaccionesLedger.Add(transaccion);

            // Registro obligatorio en AuditLog de acreditaciones de saldo
            var auditLog = new AuditLog(
                "ACREDITACION_SALDO",
                $"Acreditación de saldo por ${amount:F2} en billetera ID {billetera.Id}.",
                "Billetera",
                billetera.Id.ToString(),
                userId
            );
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync(ct);

            return new WalletResponseDto(billetera.SaldoTotal, billetera.SaldoRetenido, billetera.SaldoDisponible);
        }

        public async Task<IReadOnlyList<TransaccionLedgerDto>> ObtenerMovimientosPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default)
        {
            return await _billeteraRepository.ObtenerMovimientosPorUsuarioIdAsync(usuarioId, ct);
        }
    }
}
