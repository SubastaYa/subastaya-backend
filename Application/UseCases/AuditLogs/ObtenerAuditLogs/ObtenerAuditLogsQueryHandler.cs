using Application.DTOs;

namespace Application.UseCases.AuditLogs.ObtenerAuditLogs
{
    public class ObtenerAuditLogsQueryHandler : IQueryHandler<ObtenerAuditLogsQuery, IReadOnlyList<AuditLogDto>>
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public ObtenerAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<IReadOnlyList<AuditLogDto>> HandleAsync(ObtenerAuditLogsQuery query, CancellationToken ct = default)
        {
            return await _auditLogRepository.ObtenerLogsAsync(
                query.Entidad,
                query.EntidadId,
                query.Accion,
                query.Page,
                query.PageSize,
                ct
            );
        }
    }
}
