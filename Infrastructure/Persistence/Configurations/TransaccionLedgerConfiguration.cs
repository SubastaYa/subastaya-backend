using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Persistence.Configurations
{
    public class TransaccionLedgerConfiguration : IEntityTypeConfiguration<TransaccionLedger>
    {
        public void Configure(EntityTypeBuilder<TransaccionLedger> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Monto).HasPrecision(18, 2);

            builder.Property(t => t.Tipo)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(30);

            builder.HasOne(t => t.Billetera)
                   .WithMany()
                   .HasForeignKey(t => t.BilleteraId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Subasta)
                   .WithMany()
                   .HasForeignKey(t => t.SubastaId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
