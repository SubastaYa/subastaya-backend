using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IAuditLogRepository
    {
        Task AgregarAsync(AuditLog log, CancellationToken ct = default);
        Task<IReadOnlyList<AuditLogDto>> ObtenerLogsAsync(
            string? entidad,
            string? entidadId,
            string? accion,
            int? page,
            int? pageSize,
            CancellationToken ct = default);
    }
}
