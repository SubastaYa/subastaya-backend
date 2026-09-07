using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations
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
