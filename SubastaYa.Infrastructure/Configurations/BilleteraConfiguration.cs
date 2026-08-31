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
    public class BilleteraConfiguration : IEntityTypeConfiguration<Billetera>
    {
        public void Configure(EntityTypeBuilder<Billetera> builder)
        {
            builder.HasKey(b => b.Id);
                        
            builder.HasOne(b => b.Usuario)
                   .WithOne(u => u.Billetera)
                   .HasForeignKey<Billetera>(b => b.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);
                        
            builder.Property(b => b.SaldoTotal).HasPrecision(18, 2);
            builder.Property(b => b.SaldoRetenido).HasPrecision(18, 2);
            builder.Property(b => b.SaldoDisponible).HasPrecision(18, 2);
                        
            builder.Property(b => b.Version).IsConcurrencyToken();
        }
    }
}
