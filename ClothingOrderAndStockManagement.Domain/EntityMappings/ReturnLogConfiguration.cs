using ClothingOrderAndStockManagement.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClothingOrderAndStockManagement.Domain.EntityMappings
{
    public class ReturnLogConfiguration : IEntityTypeConfiguration<ReturnLog>
    {
        public void Configure(EntityTypeBuilder<ReturnLog> entity)
        {
            entity.HasKey(e => e.ReturnLogsId)
                .HasName("PK__ReturnLo__5879FDA276DE7257");

            entity.Property(e => e.Reason)
                .HasMaxLength(255);

            entity.Property(e => e.ReturnStatus)
                .HasMaxLength(50);

            entity.Property(e => e.UserId)
                .HasMaxLength(450);

            // EXISTING RELATIONSHIP - OrderRecord
            entity.HasOne(d => d.OrderRecords)
                .WithMany(p => p.ReturnLogs)
                .HasForeignKey(d => d.OrderRecordsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnLogs_Order");

            // ADD THIS - OrderPackage relationship (this fixes the shadow property issue)
            entity.HasOne(d => d.OrderPackage)
                .WithMany() // Assuming OrderPackage doesn't have a collection of ReturnLogs
                .HasForeignKey(d => d.OrderPackagesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnLogs_OrderPackage");

            // ADD THIS - Customer relationship
            entity.HasOne(d => d.CustomerInfo)
                .WithMany() // Assuming Customer doesn't have a ReturnLogs collection
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnLogs_Customer");

            // ADD THIS - User relationship
            entity.HasOne(d => d.User)
                .WithMany() // Assuming User doesn't have a ReturnLogs collection
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnLogs_User");
        }
    }
}
