using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations
{
    public class OfertaConfiguration : IEntityTypeConfiguration<Oferta>
    {
        public void Configure(EntityTypeBuilder<Oferta> builder)
        {
            builder.ToTable("Ofertas");
            builder.HasKey(p => p.Id);
                        
            builder.Property(p => p.Monto).HasPrecision(18, 2);
            builder.Property(p => p.FechaOferta);

            builder.HasOne(p => p.Subasta)
                   .WithMany(s => s.Ofertas)
                   .HasForeignKey(p => p.SubastaId)
                   .OnDelete(DeleteBehavior.Restrict);
                        
            builder.HasOne(p => p.Comprador)
                   .WithMany()
                   .HasForeignKey(p => p.CompradorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
