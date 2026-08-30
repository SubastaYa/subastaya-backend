using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubastaYa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Infrastructure.Configurations
{
    public class CategoriaConfiguration
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
