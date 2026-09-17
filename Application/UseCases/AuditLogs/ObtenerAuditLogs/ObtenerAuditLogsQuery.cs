using Application.DTOs;

namespace Application.UseCases.AuditLogs.ObtenerAuditLogs
{
    public record ObtenerAuditLogsQuery(
        string? Entidad = null,
        string? EntidadId = null,
        string? Accion = null,
        int? Page = null,
        int? PageSize = null
    ) : IQuery<IReadOnlyList<AuditLogDto>>;
}
