using Microsoft.EntityFrameworkCore;
using SubastaYa.Core.Entities;

namespace SubastaYa.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options){}

        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
                        
            modelBuilder.Entity<AuditLog>()
                .ToTable(tb => tb.UseSqlOutputClause(false));

            modelBuilder.Entity<AuditLog>().Property(a => a.Accion).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<AuditLog>().Property(a => a.Detalles).IsRequired();
        }
    }
}
