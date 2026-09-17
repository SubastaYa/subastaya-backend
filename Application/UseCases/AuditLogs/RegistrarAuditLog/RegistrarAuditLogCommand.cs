namespace Application.UseCases.AuditLogs.RegistrarAuditLog
{
    public record RegistrarAuditLogCommand(
        string Accion,
        string Detalles,
        string? EntidadAfectada = null,
        string? EntidadId = null,
        int? UsuarioId = null
    ) : ICommand<Guid>;
}
