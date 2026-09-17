using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IReadOnlyList<AuditLogDto>> ObtenerLogsAsync(
            string? entidad,
            string? entidadId,
            string? accion,
            int? page,
            int? pageSize,
            CancellationToken ct = default)
        {
            var query = _context.AuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(entidad))
                query = query.Where(l => l.EntidadAfectada == entidad);

            if (!string.IsNullOrWhiteSpace(entidadId))
                query = query.Where(l => l.EntidadId == entidadId);

            if (!string.IsNullOrWhiteSpace(accion))
                query = query.Where(l => l.Accion == accion);

            query = query.OrderByDescending(l => l.FechaEvento);

            int p = page.GetValueOrDefault(1);
            if (p < 1) p = 1;

            int ps = pageSize.GetValueOrDefault(20);
            if (ps < 1) ps = 20;
            if (ps > 100) ps = 100;

            query = query.Skip((p - 1) * ps).Take(ps);

            return await query.Select(l => new AuditLogDto(
                l.Id,
                l.Accion,
                l.Detalles,
                l.EntidadAfectada,
                l.EntidadId,
                l.UsuarioId,
                l.FechaEvento
            )).ToListAsync(ct);
        }
    }
}
