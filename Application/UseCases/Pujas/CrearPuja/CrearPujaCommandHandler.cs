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
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAuctionHubService _auctionHubService;
        private readonly IUnitOfWork _unitOfWork;

        public CrearPujaCommandHandler(
            ISubastaRepository subastaRepository,
            IBilleteraRepository billeteraRepository,
            IPujaRepository pujaRepository,
            IAuditLogRepository auditLogRepository,
            IUsuarioRepository usuarioRepository,
            IAuctionHubService auctionHubService,
            IUnitOfWork unitOfWork)
        {
            _subastaRepository = subastaRepository;
            _billeteraRepository = billeteraRepository;
            _pujaRepository = pujaRepository;
            _auditLogRepository = auditLogRepository;
            _usuarioRepository = usuarioRepository;
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

            // No permitir que el vendedor puje en su propia subasta
            if (subasta.VendedorId == command.CompradorId)
            {
                var auditLogVendedor = new AuditLog(
                    "INTENTO_PUJA_FALLIDO_VENDEDOR",
                    $"El vendedor intentó ofertar en su propia subasta {command.SubastaId}.",
                    "SUBASTA",
                    command.SubastaId.ToString(),
                    command.CompradorId
                );
                await _auditLogRepository.AgregarAsync(auditLogVendedor, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                throw new DomainValidationException("El vendedor no puede ofertar en su propia subasta.");
            }

            subasta.ValidarPuedeRecibirPuja(command.Monto);

            // Quién tiene la mejor puja hasta el momento
            var ultimaPuja = subasta.Pujas
                .OrderByDescending(p => p.Monto)
                .FirstOrDefault();

            // No permitir que el postor líder actual supere su propia oferta
            if (ultimaPuja != null && ultimaPuja.CompradorId == command.CompradorId)
            {
                var auditLogAutopuja = new AuditLog(
                    "INTENTO_PUJA_FALLIDO_AUTOPUJA",
                    $"El postor líder intentó superar su propia oferta en la subasta {command.SubastaId}.",
                    "SUBASTA",
                    command.SubastaId.ToString(),
                    command.CompradorId
                );
                await _auditLogRepository.AgregarAsync(auditLogAutopuja, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                throw new DomainValidationException("Ya eres el postor líder de esta subasta.");
            }

            // Se verifica la billetera del usuario que está ofertando
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

            // Se abre la transacción explícita para asegurar atomicidad entre billeteras, ledger y puja
            await _unitOfWork.BeginTransactionAsync(ct);

            // Liberación de fondos retenidos al postor anterior
            if (ultimaPuja is not null)
            {
                var billeteraAnterior = await _billeteraRepository.ObtenerPorUsuarioIdAsync(ultimaPuja.CompradorId, ct);

                if (billeteraAnterior is not null)
                {
                    billeteraAnterior.Liberar(ultimaPuja.Monto);
                    await _billeteraRepository.AgregarTransaccionLedgerAsync(
                        new TransaccionLedger(billeteraAnterior.Id, TipoTransaccion.Liberacion, ultimaPuja.Monto, command.SubastaId), ct);
                }
            }

            // Retenemos el saldo del comprador actual y registramos el movimiento
            billetera.Retener(command.Monto);
            await _billeteraRepository.AgregarTransaccionLedgerAsync(
                new TransaccionLedger(billetera.Id, TipoTransaccion.Retencion, command.Monto, command.SubastaId), ct);

            // Registramos la nueva puja ganadora
            var nuevaPuja = new Puja(command.SubastaId, command.CompradorId, command.Monto);
            await _pujaRepository.AgregarAsync(nuevaPuja, ct);

            // Extensión de tiempo por anti-sniping si la oferta entra en el último minuto
            var tiempoRestante = subasta.FechaFin - DateTime.UtcNow;
            bool tiempoExtendido = false;
            if (tiempoRestante.TotalSeconds <= 60)
            {
                subasta.ExtenderFechaFin(2);
                tiempoExtendido = true;
                await _auditLogRepository.AgregarAsync(
                    new AuditLog("EXTENSION_TIEMPO", "Extendida por regla anti-sniping", "SUBASTA", subasta.Id.ToString(), command.CompradorId), ct);
            }

            _subastaRepository.Actualizar(subasta);

            try
            {
                await _unitOfWork.CommitTransactionAsync(ct);
            }
            catch (Exception ex) when (ex.GetType().Name == "DbUpdateConcurrencyException")
            {
                var auditLogConcurrencia = new AuditLog(
                    "INTENTO_PUJA_FALLIDO_CONCURRENCIA",
                    $"Oferta de ${command.Monto:F2} rechazada por colisión de concurrencia optimista en la subasta ID {command.SubastaId}.",
                    "SUBASTA",
                    command.SubastaId.ToString(),
                    command.CompradorId
                );

                await _auditLogRepository.AgregarAsync(auditLogConcurrencia, CancellationToken.None);
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                throw;
            }

            var comprador = await _usuarioRepository.ObtenerPorIdAsync(command.CompradorId, ct);
            var nombreOfuscado = UsuarioHelper.OfuscarNombre(comprador?.Nombre);

            // Se notifica a todos los postores conectados en tiempo real por SignalR
            await _auctionHubService.BroadcastNuevaPujaAsync(
                command.SubastaId,
                command.Monto,
                nombreOfuscado,
                nuevaPuja.FechaPuja,
                command.CompradorId
            );

            if (tiempoExtendido)
            {
                await _auctionHubService.BroadcastExtensionTiempoAsync(subasta.Id, subasta.FechaFin);
            }

            return nuevaPuja.Id;
        }
    }
}
