using Application.Interfaces;
using Application.Interfaces.Services;
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
        private readonly IAuctionHubService _auctionHubService;
        private readonly IUnitOfWork _unitOfWork;

        public CrearPujaCommandHandler(
            ISubastaRepository subastaRepository,
            IBilleteraRepository billeteraRepository,
            IPujaRepository pujaRepository,
            IAuditLogRepository auditLogRepository,
            IAuctionHubService auctionHubService,
            IUnitOfWork unitOfWork)
        {
            _subastaRepository = subastaRepository;
            _billeteraRepository = billeteraRepository;
            _pujaRepository = pujaRepository;
            _auditLogRepository = auditLogRepository;
            _auctionHubService = auctionHubService;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> HandleAsync(CrearPujaCommand command, CancellationToken ct = default)
        {
            if (command.Monto <= 0)
            {
                throw new ArgumentException("El monto de la puja debe ser mayor a cero.", nameof(command.Monto));
            }

            var subasta = await _subastaRepository.ObtenerConPujasPorIdAsync(command.SubastaId, ct)
                ?? throw new KeyNotFoundException($"La subasta con ID {command.SubastaId} no existe.");

            subasta.ValidarPuedeRecibirPuja(command.Monto);

            var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.CompradorId, ct)
                ?? throw new KeyNotFoundException($"No se encontró la billetera para el usuario con ID {command.CompradorId}.");

            if (billetera.SaldoDisponible < command.Monto)
            {
                var auditLogFallo = new AuditLog(
                    "INTENTO_PUJA_FALLIDO_SALDO",
                    $"Saldo insuficiente en la subasta {command.SubastaId}. Requerido: {command.Monto}, Disponible: {billetera.SaldoDisponible}.",
                    "Billetera",
                    billetera.Id.ToString(),
                    command.CompradorId
                );
                await _auditLogRepository.AgregarAsync(auditLogFallo, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                throw new SaldoInsuficienteException($"Saldo insuficiente para realizar la oferta. Saldo disponible: {billetera.SaldoDisponible}, monto requerido: {command.Monto}.");
            }

            await _unitOfWork.BeginTransactionAsync(ct);

            var ultimaPuja = subasta.Pujas
                .OrderByDescending(p => p.Monto)
                .FirstOrDefault();

            if (ultimaPuja is not null)
            {
                var billeteraAnterior = (ultimaPuja.CompradorId == command.CompradorId)
                    ? billetera
                    : await _billeteraRepository.ObtenerPorUsuarioIdAsync(ultimaPuja.CompradorId, ct);

                if (billeteraAnterior is not null)
                {
                    billeteraAnterior.Liberar(ultimaPuja.Monto);
                    await _billeteraRepository.AgregarTransaccionLedgerAsync(
                        new TransaccionLedger(billeteraAnterior.Id, TipoTransaccion.Liberacion, ultimaPuja.Monto, command.SubastaId), ct);
                }
            }

            billetera.Retener(command.Monto);
            await _billeteraRepository.AgregarTransaccionLedgerAsync(
                new TransaccionLedger(billetera.Id, TipoTransaccion.Retencion, command.Monto, command.SubastaId), ct);

            var nuevaPuja = new Puja(command.SubastaId, command.CompradorId, command.Monto);
            await _pujaRepository.AgregarAsync(nuevaPuja, ct);

            var huboExtension = false;
            if ((subasta.FechaFin - DateTime.UtcNow).TotalSeconds <= 60)
            {
                subasta.ExtenderFechaFin(2);
                huboExtension = true;
                await _auditLogRepository.AgregarAsync(
                    new AuditLog("EXTENSION_TIEMPO", "Extendida por regla anti-sniping", "SUBASTA", subasta.Id.ToString()), ct);
            }

            _subastaRepository.Actualizar(subasta);

            await _unitOfWork.CommitTransactionAsync(ct);

            await _auctionHubService.BroadcastNuevaPujaAsync(
                command.SubastaId,
                command.Monto,
                $"Usuario #{command.CompradorId}",
                nuevaPuja.FechaPuja
            );

            if (huboExtension)
            {
                await _auctionHubService.BroadcastExtensionTiempoAsync(command.SubastaId, subasta.FechaFin);
            }

            return nuevaPuja.Id;
        }
    }
}
