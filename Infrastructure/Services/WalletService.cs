using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services
{
    public class WalletService : IWalletService
    {
        private readonly IBilleteraRepository _billeteraRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public WalletService(
            IBilleteraRepository billeteraRepository,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
        {
            _billeteraRepository = billeteraRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<WalletResponseDto> GetBalanceAsync(int userId, CancellationToken ct = default)
        {
            var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(userId, ct)
                ?? throw new KeyNotFoundException($"No se encontró la billetera para el usuario con ID {userId}.");

            return new WalletResponseDto(billetera.SaldoTotal, billetera.SaldoRetenido, billetera.SaldoDisponible);
        }

        public async Task<WalletResponseDto> DepositAsync(int userId, decimal amount, CancellationToken ct = default)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("El monto a depositar debe ser mayor a cero.", nameof(amount));
            }

            var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(userId, ct)
                ?? throw new KeyNotFoundException($"No se encontró la billetera para el usuario con ID {userId}.");

            billetera.Depositar(amount);

            var transaccion = new TransaccionLedger(billetera.Id, TipoTransaccion.Deposito, amount);
            await _billeteraRepository.AgregarTransaccionLedgerAsync(transaccion, ct);

            // Registro obligatorio en AuditLog de acreditaciones manuales de saldo (PDF Pág. 4, Secc. 2.4)
            var auditLog = new AuditLog(
                "ACREDITACION_SALDO",
                $"Acreditación de saldo por ${amount:F2} en billetera ID {billetera.Id}.",
                "Billetera",
                billetera.Id.ToString(),
                userId
            );
            await _auditLogRepository.AgregarAsync(auditLog, ct);

            await _unitOfWork.SaveChangesAsync(ct);

            return new WalletResponseDto(billetera.SaldoTotal, billetera.SaldoRetenido, billetera.SaldoDisponible);
        }

        public async Task<IReadOnlyList<TransaccionLedgerDto>> ObtenerMovimientosPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default)
        {
            return await _billeteraRepository.ObtenerMovimientosPorUsuarioIdAsync(usuarioId, ct);
        }
    }
}
