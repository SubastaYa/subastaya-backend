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
    public class UsuarioConfiguration: IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
            builder.Property(u => u.PasswordHash).IsRequired();

            builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
            builder.HasIndex(u => u.Email).IsUnique();
        }
    }
}
