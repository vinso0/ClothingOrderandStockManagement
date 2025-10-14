using ClothingOrderAndStockManagement.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClothingOrderAndStockManagement.Infrastructure.Data.EntityMappings
{
    public class OrderPackageConfiguration : IEntityTypeConfiguration<OrderPackage>
    {
        public void Configure(EntityTypeBuilder<OrderPackage> entity)
        {
            entity.HasKey(e => e.OrderPackagesId).HasName("PK__OrderPac__2010BE6808487A3F");

            entity.HasOne(d => d.OrderRecords).WithMany(p => p.OrderPackages)
                .HasForeignKey(d => d.OrderRecordsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderPackages_OrderRecords");

            entity.HasOne(d => d.Packages).WithMany(p => p.OrderPackages)
                .HasForeignKey(d => d.PackagesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderPackages_Packages");
        }
    }
}
