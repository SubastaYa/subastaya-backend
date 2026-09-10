using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations
{
    public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
            builder.HasKey(c => c.Id);
                        
            builder.Property(c => c.Nombre)
                   .IsRequired()
                   .HasMaxLength(50);
                        
            builder.Property(c => c.UrlIcono)
                   .IsRequired()
                   .HasMaxLength(300);
        }
    }
}
