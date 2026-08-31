using Microsoft.EntityFrameworkCore;
using SubastaYa.Core.Entities;

namespace SubastaYa.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options){}

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Billetera> Billeteras { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Subasta> Subastas { get; set; }
        public DbSet<Puja> Pujas { get; set; }
        public DbSet<TransaccionLedger> TransaccionesLedger { get; set; }



        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
                                    
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
