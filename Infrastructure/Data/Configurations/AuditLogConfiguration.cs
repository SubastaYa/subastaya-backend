using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {            
            builder.ToTable(tb => tb.UseSqlOutputClause(false));

            builder.Property(a => a.Accion).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Detalles).IsRequired();
            builder.Property(a => a.UsuarioId).IsRequired(false);
            builder.HasIndex(a => a.FechaEvento);
        }
    }
}
