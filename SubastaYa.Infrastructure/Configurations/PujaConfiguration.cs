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
    public class PujaConfiguration : IEntityTypeConfiguration<Puja>
    {
        public void Configure(EntityTypeBuilder<Puja> builder)
        {
            builder.HasKey(p => p.Id);
                        
            builder.Property(p => p.Monto).HasPrecision(18, 2);

            builder.HasOne(p => p.Subasta)
                   .WithMany(s => s.Pujas)
                   .HasForeignKey(p => p.SubastaId)
                   .OnDelete(DeleteBehavior.Restrict);
                        
            builder.HasOne(p => p.Comprador)
                   .WithMany()
                   .HasForeignKey(p => p.CompradorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
