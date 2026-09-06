using Domain.Entities;

namespace Application.Interfaces
{
    public interface IAuditLogRepository
    {
        Task AgregarAsync(AuditLog log, CancellationToken ct = default);
    }
}
