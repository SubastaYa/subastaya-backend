using Application.Interfaces;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.UseCases.Ofertas.CrearOferta
{
    public class CrearOfertaCommandHandler : ICommandHandler<CrearOfertaCommand, int>
    {
        private readonly ISubastaRepository _subastaRepository;
        private readonly IBilleteraRepository _billeteraRepository;
        private readonly IOfertaRepository _ofertaRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAuctionHubService _auctionHubService;
        private readonly IUnitOfWork _unitOfWork;

        public CrearOfertaCommandHandler(
            ISubastaRepository subastaRepository,
            IBilleteraRepository billeteraRepository,
            IOfertaRepository ofertaRepository,
            IAuditLogRepository auditLogRepository,
            IUsuarioRepository usuarioRepository,
            IAuctionHubService auctionHubService,
            IUnitOfWork unitOfWork)
        {
            _subastaRepository = subastaRepository;
            _billeteraRepository = billeteraRepository;
            _ofertaRepository = ofertaRepository;
            _auditLogRepository = auditLogRepository;
            _usuarioRepository = usuarioRepository;
            _auctionHubService = auctionHubService;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> HandleAsync(CrearOfertaCommand command, CancellationToken ct = default)
        {
            if (command.Monto <= 0)
            {
                throw new ArgumentException("El monto de la oferta debe ser mayor a cero.", nameof(command.Monto));
            }

            var subasta = await _subastaRepository.ObtenerConOfertasPorIdAsync(command.SubastaId, ct)
                ?? throw new KeyNotFoundException($"La subasta con ID {command.SubastaId} no existe.");

            // No permitir que el vendedor oferte en su propia subasta
            if (subasta.VendedorId == command.CompradorId)
            {
                var auditLogVendedor = new AuditLog(
                    "INTENTO_OFERTA_FALLIDO_VENDEDOR",
                    $"El vendedor intentó ofertar en su propia subasta {command.SubastaId}.",
                    "SUBASTA",
                    command.SubastaId.ToString(),
                    command.CompradorId
                );
                await _auditLogRepository.AgregarAsync(auditLogVendedor, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                throw new DomainValidationException("El vendedor no puede ofertar en su propia subasta.");
            }

            subasta.ValidarPuedeRecibirOferta(command.Monto);

            // Quién tiene la mejor oferta hasta el momento
            var ultimaOferta = subasta.Ofertas
                .OrderByDescending(p => p.Monto)
                .FirstOrDefault();

            // No permitir que el postor líder actual supere su propia oferta
            if (ultimaOferta != null && ultimaOferta.CompradorId == command.CompradorId)
            {
                var auditLogAutooferta = new AuditLog(
                    "INTENTO_OFERTA_FALLIDO_AUTOOFERTA",
                    $"El postor líder intentó superar su propia oferta en la subasta {command.SubastaId}.",
                    "SUBASTA",
                    command.SubastaId.ToString(),
                    command.CompradorId
                );
                await _auditLogRepository.AgregarAsync(auditLogAutooferta, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                throw new DomainValidationException("Ya eres el postor líder de esta subasta.");
            }

            // Se verifica la billetera del usuario que está ofertando
            var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.CompradorId, ct)
                ?? throw new KeyNotFoundException($"No se encontró la billetera para el usuario con ID {command.CompradorId}.");

            if (billetera.SaldoDisponible < command.Monto)
            {
                var auditLogFallo = new AuditLog(
                    "INTENTO_OFERTA_FALLIDO_SALDO",
                    $"Saldo insuficiente en la subasta {command.SubastaId}. Requerido: {command.Monto}, Disponible: {billetera.SaldoDisponible}.",
                    "Billetera",
                    billetera.Id.ToString(),
                    command.CompradorId
                );
                await _auditLogRepository.AgregarAsync(auditLogFallo, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                throw new SaldoInsuficienteException($"Saldo insuficiente para realizar la oferta. Saldo disponible: {billetera.SaldoDisponible}, monto requerido: {command.Monto}.");
            }

            // Se abre la transacción explícita para asegurar atomicidad entre billeteras, ledger y oferta
            await _unitOfWork.BeginTransactionAsync(ct);

            // Liberación de fondos retenidos al postor anterior
            if (ultimaOferta is not null)
            {
                var billeteraAnterior = await _billeteraRepository.ObtenerPorUsuarioIdAsync(ultimaOferta.CompradorId, ct);

                if (billeteraAnterior is not null)
                {
                    billeteraAnterior.Liberar(ultimaOferta.Monto);
                    await _billeteraRepository.AgregarTransaccionLedgerAsync(
                        new TransaccionLedger(billeteraAnterior.Id, TipoTransaccion.Liberacion, ultimaOferta.Monto, command.SubastaId), ct);
                }
            }

            // Se retiene el saldo del comprador actual y se registra el movimiento
            billetera.Retener(command.Monto);
            await _billeteraRepository.AgregarTransaccionLedgerAsync(
                new TransaccionLedger(billetera.Id, TipoTransaccion.Retencion, command.Monto, command.SubastaId), ct);

            var nuevaOferta = new Oferta(command.SubastaId, command.CompradorId, command.Monto);
            await _ofertaRepository.AgregarAsync(nuevaOferta, ct);

            // Se extiende el tiempo por regla anti-sniping si la oferta entra en el último minuto
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
                    "INTENTO_OFERTA_FALLIDO_CONCURRENCIA",
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
            await _auctionHubService.BroadcastNuevaOfertaAsync(
                command.SubastaId,
                command.Monto,
                nombreOfuscado,
                nuevaOferta.FechaOferta,
                command.CompradorId
            );

            if (tiempoExtendido)
            {
                await _auctionHubService.BroadcastExtensionTiempoAsync(subasta.Id, subasta.FechaFin);
            }

            return nuevaOferta.Id;
        }
    }
}
