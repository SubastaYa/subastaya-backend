using Domain.Entities;
using Domain.Enums;

namespace Application.UseCases.Subastas.LiquidarSubastasVencidas
{
    public class LiquidarSubastasVencidasCommandHandler : ICommandHandler<LiquidarSubastasVencidasCommand>
    {
        private readonly ISubastaRepository _subastaRepository;
        private readonly IBilleteraRepository _billeteraRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IAuctionHubService _auctionHubService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LiquidarSubastasVencidasCommandHandler> _logger;

        public LiquidarSubastasVencidasCommandHandler(
            ISubastaRepository subastaRepository,
            IBilleteraRepository billeteraRepository,
            IAuditLogRepository auditLogRepository,
            IAuctionHubService auctionHubService,
            IUnitOfWork unitOfWork,
            ILogger<LiquidarSubastasVencidasCommandHandler> logger)
        {
            _subastaRepository = subastaRepository;
            _billeteraRepository = billeteraRepository;
            _auditLogRepository = auditLogRepository;
            _auctionHubService = auctionHubService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task HandleAsync(LiquidarSubastasVencidasCommand command, CancellationToken ct = default)
        {
            var ahora = DateTime.UtcNow;

            // 1. Activar subastas programadas que alcanzaron su fecha de inicio
            var paraActivar = await _subastaRepository.ObtenerProgramadasParaIniciarAsync(ahora, ct);
            foreach (var subasta in paraActivar)
            {
                subasta.Activar();
                _subastaRepository.Actualizar(subasta);

                await _auditLogRepository.AgregarAsync(new AuditLog(
                    "ACTIVACION_WORKER",
                    $"Subasta activada automáticamente al alcanzar su fecha de inicio ({subasta.FechaInicio:u}).",
                    "SUBASTA",
                    subasta.Id.ToString()
                ), ct);
            }

            if (paraActivar.Any())
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }

            // 2. Obtener subastas vencidas para liquidar
            var vencidas = await _subastaRepository.ObtenerVencidasParaLiquidacionAsync(ahora, ct);

            foreach (var subasta in vencidas)
            {
                try
                {
                    await _unitOfWork.BeginTransactionAsync(ct);

                    var ofertaGanadora = subasta.Ofertas.OrderByDescending(p => p.Monto).FirstOrDefault();

                    if (ofertaGanadora is not null)
                    {
                        var billeteraComprador = await _billeteraRepository.ObtenerPorUsuarioIdAsync(ofertaGanadora.CompradorId, ct)
                            ?? throw new KeyNotFoundException($"Billetera del comprador {ofertaGanadora.CompradorId} no encontrada.");

                        var billeteraVendedor = await _billeteraRepository.ObtenerPorUsuarioIdAsync(subasta.VendedorId, ct)
                            ?? throw new KeyNotFoundException($"Billetera del vendedor {subasta.VendedorId} no encontrada.");

                        // Debitar el monto del saldo retenido del comprador
                        billeteraComprador.DebitarRetenido(ofertaGanadora.Monto);
                        await _billeteraRepository.AgregarTransaccionLedgerAsync(
                            new TransaccionLedger(billeteraComprador.Id, TipoTransaccion.Pago, ofertaGanadora.Monto, subasta.Id), ct);

                        // Acreditar el monto como saldo disponible al vendedor
                        billeteraVendedor.AcreditarCobro(ofertaGanadora.Monto);
                        await _billeteraRepository.AgregarTransaccionLedgerAsync(
                            new TransaccionLedger(billeteraVendedor.Id, TipoTransaccion.Cobro, ofertaGanadora.Monto, subasta.Id), ct);

                        subasta.Finalizar();
                        _subastaRepository.Actualizar(subasta);

                        // Registrar log de auditoría inmutable
                        await _auditLogRepository.AgregarAsync(new AuditLog(
                            "CIERRE_WORKER_VENTA",
                            $"Subasta adjudicada por ${ofertaGanadora.Monto} al comprador {ofertaGanadora.CompradorId}.",
                            "SUBASTA",
                            subasta.Id.ToString(),
                            ofertaGanadora.CompradorId
                        ), ct);

                        await _unitOfWork.CommitTransactionAsync(ct);

                        // Notificación en tiempo real del resultado de adjudicación
                        var ganadorSeudonimo = UsuarioHelper.OfuscarNombre(ofertaGanadora.Comprador?.Nombre);
                        await _auctionHubService.BroadcastSubastaFinalizadaAsync(subasta.Id, "Finalizada", ganadorSeudonimo, ofertaGanadora.Monto);
                    }
                    else
                    {
                        subasta.MarcarDesierta();
                        _subastaRepository.Actualizar(subasta);

                        await _auditLogRepository.AgregarAsync(new AuditLog(
                            "CIERRE_WORKER_DESIERTA",
                            "Subasta finalizada sin ofertas registradas.",
                            "SUBASTA",
                            subasta.Id.ToString()
                        ), ct);

                        await _unitOfWork.CommitTransactionAsync(ct);

                        // Notificación en tiempo real de subasta desierta
                        await _auctionHubService.BroadcastSubastaFinalizadaAsync(subasta.Id, "Desierta", null, null);
                    }
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    _logger.LogError(ex, "Error al procesar y liquidar la subasta ID {SubastaId}", subasta.Id);
                }
            }
        }
    }
}
