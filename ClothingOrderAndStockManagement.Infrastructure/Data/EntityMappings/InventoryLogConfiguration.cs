using ClothingOrderAndStockManagement.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClothingOrderAndStockManagement.Infrastructure.Data.EntityMappings
{
    public class InventoryLogConfiguration : IEntityTypeConfiguration<InventoryLog>
    {
        public void Configure(EntityTypeBuilder<InventoryLog> entity)
        {
            entity.HasKey(e => e.LogId).HasName("PK__Inventor__5E548648A8955A45");

            entity.Property(e => e.ChangeType).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.Item).WithMany(p => p.InventoryLogs)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_InventoryLogs_Item");

            entity.HasOne(d => d.PackageItem).WithMany(p => p.InventoryLogs)
                .HasForeignKey(d => d.PackageItemId)
                .HasConstraintName("FK_InventoryLogs_PackageItem");
        }
    }
}
