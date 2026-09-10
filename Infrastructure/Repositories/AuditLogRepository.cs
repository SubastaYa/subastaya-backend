using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AgregarAsync(AuditLog log, CancellationToken ct = default)
        {
            await _context.AuditLogs.AddAsync(log, ct);
        }
    }
}
