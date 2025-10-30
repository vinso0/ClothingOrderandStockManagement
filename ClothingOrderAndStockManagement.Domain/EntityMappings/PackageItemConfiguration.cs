using ClothingOrderAndStockManagement.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClothingOrderAndStockManagement.Domain.EntityMappings
{
    public class PackageItemConfiguration : IEntityTypeConfiguration<PackageItem>
    {
        public void Configure(EntityTypeBuilder<PackageItem> entity)
        {
            entity.HasKey(e => e.PackageItemId).HasName("PK__PackageI__D45F71B12F5A8970");

            entity.ToTable("PackageItem");

            entity.HasOne(d => d.Item).WithMany(p => p.PackageItems)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PackageItem_Item");

            entity.HasOne(d => d.Packages).WithMany(p => p.PackageItems)
                .HasForeignKey(d => d.PackagesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PackageItem_Packages");
        }
    }
}
