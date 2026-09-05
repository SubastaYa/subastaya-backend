using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Persistence.Configurations
{
    public class SubastaConfiguration : IEntityTypeConfiguration<Subasta>
    {
        public void Configure(EntityTypeBuilder<Subasta> builder) 
        {
            builder.HasKey(s => s.Id);
                        
            builder.Property(s => s.Titulo).IsRequired().HasMaxLength(100);
            builder.Property(s => s.Descripcion).IsRequired().HasMaxLength(1000);
            builder.Property(s => s.UrlImagen).IsRequired().HasMaxLength(255);
                        
            builder.Property(s => s.PrecioBase).HasPrecision(18, 2);
            builder.Property(s => s.IncrementoMinimo).HasPrecision(18, 2);
                        
            builder.Property(s => s.Estado)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);
                        
            builder.Property(s => s.Version).IsConcurrencyToken();
                        
            builder.HasOne(s => s.Categoria)
                   .WithMany(c => c.Subastas)
                   .HasForeignKey(s => s.CategoriaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Vendedor)
                   .WithMany()
                   .HasForeignKey(s => s.VendedorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
