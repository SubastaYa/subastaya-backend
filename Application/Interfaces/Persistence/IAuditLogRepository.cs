using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IAuditLogRepository
    {
        Task AgregarAsync(AuditLog log, CancellationToken ct = default);
    }
}
