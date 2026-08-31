using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubastaYa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Infrastructure.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {            
            builder.ToTable(tb => tb.UseSqlOutputClause(false));

            builder.Property(a => a.Accion).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Detalles).IsRequired();
        }
    }
}
