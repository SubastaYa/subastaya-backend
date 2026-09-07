using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.UseCases.Pujas.CrearPuja
{
    public class CrearPujaCommandHandler : ICommandHandler<CrearPujaCommand, int>
    {
        private readonly ISubastaRepository _subastaRepository;
        private readonly IBilleteraRepository _billeteraRepository;
        private readonly IPujaRepository _pujaRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CrearPujaCommandHandler(
            ISubastaRepository subastaRepository,
            IBilleteraRepository billeteraRepository,
            IPujaRepository pujaRepository,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
        {
            _subastaRepository = subastaRepository;
            _billeteraRepository = billeteraRepository;
            _pujaRepository = pujaRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> HandleAsync(CrearPujaCommand command, CancellationToken ct = default)
        {
            if (command.Monto <= 0)
            {
                throw new ArgumentException("El monto de la puja debe ser mayor a cero.", nameof(command.Monto));
            }

            var subasta = await _subastaRepository.ObtenerConPujasPorIdAsync(command.SubastaId, ct);
            if (subasta is null)
            {
                throw new KeyNotFoundException($"La subasta con ID {command.SubastaId} no existe.");
            }

            subasta.ValidarPuedeRecibirPuja(command.Monto);

            var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.CompradorId, ct);
            if (billetera is null)
            {
                throw new KeyNotFoundException($"No se encontró la billetera para el usuario con ID {command.CompradorId}.");
            }

            if (billetera.SaldoDisponible < command.Monto)
            {
                var auditLogFallo = new AuditLog(
                    "INTENTO_PUJA_FALLIDO_SALDO",
                    $"Saldo insuficiente para el usuario {command.CompradorId} en la subasta {command.SubastaId}. Requerido: {command.Monto}, Disponible: {billetera.SaldoDisponible}.",
                    "Billetera",
                    billetera.Id.ToString()
                );
                await _auditLogRepository.AgregarAsync(auditLogFallo, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                throw new SaldoInsuficienteException($"Saldo insuficiente para realizar la oferta. Saldo disponible: {billetera.SaldoDisponible}, monto requerido: {command.Monto}.");
            }

            await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var ultimaPuja = subasta.Pujas != null && subasta.Pujas.Any()
                    ? subasta.Pujas.OrderByDescending(p => p.Monto).ThenByDescending(p => p.FechaPuja).FirstOrDefault()
                    : null;

                if (ultimaPuja != null)
                {
                    var billeteraAnterior = (ultimaPuja.CompradorId == command.CompradorId)
                        ? billetera
                        : await _billeteraRepository.ObtenerPorUsuarioIdAsync(ultimaPuja.CompradorId, ct);

                    if (billeteraAnterior != null)
                    {
                        billeteraAnterior.Liberar(ultimaPuja.Monto);
                        var transaccionLiberacion = new TransaccionLedger(
                            billeteraAnterior.Id,
                            TipoTransaccion.Liberacion,
                            ultimaPuja.Monto,
                            command.SubastaId
                        );
                        await _billeteraRepository.AgregarTransaccionLedgerAsync(transaccionLiberacion, ct);
                    }
                }

                billetera.Retener(command.Monto);
                var transaccionRetencion = new TransaccionLedger(
                    billetera.Id,
                    TipoTransaccion.Retencion,
                    command.Monto,
                    command.SubastaId
                );
                await _billeteraRepository.AgregarTransaccionLedgerAsync(transaccionRetencion, ct);

                var nuevaPuja = new Puja(command.SubastaId, command.CompradorId, command.Monto);
                await _pujaRepository.AgregarAsync(nuevaPuja, ct);

                var tiempoRestante = subasta.FechaFin - DateTime.UtcNow;
                if (tiempoRestante.TotalSeconds <= 60)
                {
                    subasta.ExtenderFechaFin(2);
                    var auditLogAntiSniping = new AuditLog(
                        "EXTENSION_TIEMPO",
                        "Extendida por regla anti-sniping",
                        "SUBASTA",
                        subasta.Id.ToString()
                    );
                    await _auditLogRepository.AgregarAsync(auditLogAntiSniping, ct);
                }

                _subastaRepository.Actualizar(subasta);

                await _unitOfWork.CommitTransactionAsync(ct);
                return nuevaPuja.Id;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }
    }
}
