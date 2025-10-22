using ClothingOrderAndStockManagement.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClothingOrderAndStockManagement.Infrastructure.Data.EntityMappings
{
    public class PaymentRecordConfiguration : IEntityTypeConfiguration<PaymentRecord>
    {
        public void Configure(EntityTypeBuilder<PaymentRecord> entity)
        {
            entity.HasKey(e => e.PaymentRecordsId)
                .HasName("PK__PaymentR__A4C5C698D3DD885B");

            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)");

            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50);

            // INCREASE FROM 500 TO 500 CHARACTERS
            entity.Property(e => e.ProofUrl)
                .HasMaxLength(500);

            entity.Property(e => e.ProofUrl2)
                .HasMaxLength(500);

            entity.Property(e => e.PaymentDate)
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            entity.HasOne(d => d.OrderRecords)
                .WithMany(p => p.PaymentRecords)
                .HasForeignKey(d => d.OrderRecordsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaymentRecords_Order");
        }

    }
}
