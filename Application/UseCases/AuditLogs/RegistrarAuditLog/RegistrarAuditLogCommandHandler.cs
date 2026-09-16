using Domain.Entities;

namespace Application.UseCases.AuditLogs.RegistrarAuditLog
{
    public class RegistrarAuditLogCommandHandler : ICommandHandler<RegistrarAuditLogCommand, Guid>
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegistrarAuditLogCommandHandler(IAuditLogRepository auditLogRepository, IUnitOfWork unitOfWork)
        {
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> HandleAsync(RegistrarAuditLogCommand command, CancellationToken ct = default)
        {
            var log = new AuditLog(
                command.Accion,
                command.Detalles,
                command.EntidadAfectada,
                command.EntidadId,
                command.UsuarioId
            );

            await _auditLogRepository.AgregarAsync(log, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return log.Id;
        }
    }
}
