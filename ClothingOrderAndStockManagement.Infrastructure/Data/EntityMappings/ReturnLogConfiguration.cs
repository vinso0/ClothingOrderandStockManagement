using ClothingOrderAndStockManagement.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClothingOrderAndStockManagement.Infrastructure.Data.EntityMappings
{
    public class ReturnLogConfiguration : IEntityTypeConfiguration<ReturnLog>
    {
        public void Configure(EntityTypeBuilder<ReturnLog> entity)
        {
            entity.HasKey(e => e.ReturnLogsId).HasName("PK__ReturnLo__5879FDA276DE7257");

            entity.Property(e => e.Reason).HasMaxLength(255);
            entity.Property(e => e.ReturnStatus).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.OrderRecords).WithMany(p => p.ReturnLogs)
                .HasForeignKey(d => d.OrderRecordsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnLogs_Order");
        }
    }
}
