using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;

namespace Application.UseCases.Billetera.DepositarFondos
{
    public class DepositarFondosCommandHandler : ICommandHandler<DepositarFondosCommand, WalletResponseDto>
    {
        private readonly IBilleteraRepository _billeteraRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DepositarFondosCommandHandler(
            IBilleteraRepository billeteraRepository,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
        {
            _billeteraRepository = billeteraRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<WalletResponseDto> HandleAsync(DepositarFondosCommand command, CancellationToken ct = default)
        {
            if (command.Monto <= 0)
            {
                throw new ArgumentException("El monto a depositar debe ser mayor a cero.", nameof(command.Monto));
            }

            var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.UsuarioId, ct)
                ?? throw new KeyNotFoundException($"No se encontró la billetera para el usuario con ID {command.UsuarioId}.");

            billetera.Depositar(command.Monto);

            var transaccion = new TransaccionLedger(billetera.Id, TipoTransaccion.Deposito, command.Monto);
            await _billeteraRepository.AgregarTransaccionLedgerAsync(transaccion, ct);

            // Registro obligatorio en AuditLog de acreditaciones manuales de saldo (PDF Pág. 4, Secc. 2.4)
            var auditLog = new AuditLog(
                "ACREDITACION_SALDO",
                $"Acreditación de saldo por ${command.Monto:F2} en billetera ID {billetera.Id}.",
                "Billetera",
                billetera.Id.ToString(),
                command.UsuarioId
            );
            await _auditLogRepository.AgregarAsync(auditLog, ct);

            await _unitOfWork.SaveChangesAsync(ct);

            return new WalletResponseDto(billetera.SaldoTotal, billetera.SaldoRetenido, billetera.SaldoDisponible);
        }
    }
}
