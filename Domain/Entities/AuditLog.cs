namespace Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; private set; }
        public string Accion { get; private set; } = string.Empty;
        public string Detalles { get; private set; } = string.Empty;
        public string? EntidadAfectada { get; private set; }
        public string? EntidadId { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public DateTime FechaEvento { get; private set; }

        public AuditLog(string accion, string detalles, string? entidadAfectada = null, string? entidadId = null, Guid? usuarioId = null)
        {
            Id = Guid.NewGuid();
            Accion = accion;
            Detalles = detalles;
            EntidadAfectada = entidadAfectada;
            EntidadId = entidadId;
            UsuarioId = usuarioId;
            FechaEvento = DateTime.UtcNow;
        }

        protected AuditLog() { }
    }
}
