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
            // Validamos que el monto ofertado sea válido
            if (command.Monto <= 0)
            {
                throw new ArgumentException("El monto de la puja debe ser mayor a cero.", nameof(command.Monto));
            }

            // Obtenemos la subasta con sus pujas para chequear el estado y el precio actual
            var subasta = await _subastaRepository.ObtenerConPujasPorIdAsync(command.SubastaId, ct)
                ?? throw new KeyNotFoundException($"La subasta con ID {command.SubastaId} no existe.");

            // Valida que esté activa, no vencida y que supere el monto mínimo (última puja + incremento)
            subasta.ValidarPuedeRecibirPuja(command.Monto);

            // Verificamos la billetera del usuario que está ofertando
            var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.CompradorId, ct)
                ?? throw new KeyNotFoundException($"No se encontró la billetera para el usuario con ID {command.CompradorId}.");

            // Control de saldo: si no tiene suficiente saldo disponible, guardamos auditoría y cortamos
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

            // Abrimos transacción explícita para asegurar atomicidad entre billeteras, ledger y puja
            await _unitOfWork.BeginTransactionAsync(ct);

            // Buscamos quién tenía la mejor puja hasta el momento
            var ultimaPuja = subasta.Pujas
                .OrderByDescending(p => p.Monto)
                .FirstOrDefault();

            // Si había un postor anterior, le liberamos los fondos retenidos
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

            // Retenemos el saldo del comprador actual y registramos el movimiento
            billetera.Retener(command.Monto);
            await _billeteraRepository.AgregarTransaccionLedgerAsync(
                new TransaccionLedger(billetera.Id, TipoTransaccion.Retencion, command.Monto, command.SubastaId), ct);

            // Registramos la nueva puja ganadora
            var nuevaPuja = new Puja(command.SubastaId, command.CompradorId, command.Monto);
            await _pujaRepository.AgregarAsync(nuevaPuja, ct);

            // Si la oferta entra en el último minuto, extendemos 2 minutos para evitar sniping
            var tiempoRestante = subasta.FechaFin - DateTime.UtcNow;
            bool tiempoExtendido = false;
            if (tiempoRestante.TotalSeconds <= 60)
            {
                subasta.ExtenderFechaFin(2);
                tiempoExtendido = true;
                await _auditLogRepository.AgregarAsync(
                    new AuditLog("EXTENSION_TIEMPO", "Extendida por regla anti-sniping", "SUBASTA", subasta.Id.ToString()), ct);
            }

            // Marcamos la subasta como actualizada para que EF Core valide el RowVersion en el UPDATE
            _subastaRepository.Actualizar(subasta);

            await _unitOfWork.CommitTransactionAsync(ct);

            // Obtenemos los datos del comprador para ofuscar su nombre en la transmisión en vivo
            var comprador = await _usuarioRepository.ObtenerPorIdAsync(command.CompradorId, ct);
            var nombreOfuscado = OfuscarNombre(comprador?.Nombre);

            // Notificamos a todos los postores conectados en tiempo real por SignalR
            await _auctionHubService.BroadcastNuevaPujaAsync(
                command.SubastaId,
                command.Monto,
                nombreOfuscado,
                nuevaPuja.FechaPuja
            );

            if (tiempoExtendido)
            {
                await _auctionHubService.BroadcastExtensionTiempoAsync(subasta.Id, subasta.FechaFin);
            }

            return nuevaPuja.Id;
        }

        private static string OfuscarNombre(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return "Anónimo";
            }

            var partes = nombre.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var ofuscados = new List<string>();

            foreach (var parte in partes)
            {
                if (parte.Length > 1)
                {
                    ofuscados.Add(parte[0] + "***");
                }
                else
                {
                    ofuscados.Add(parte);
                }
            }

            return string.Join(" ", ofuscados);
        }
    }
}
