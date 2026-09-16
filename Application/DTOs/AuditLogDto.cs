namespace Application.DTOs
{
    public record AuditLogDto(
        Guid Id,
        string Accion,
        string Detalles,
        string? EntidadAfectada,
        string? EntidadId,
        int? UsuarioId,
        DateTime FechaEvento
    );
}
