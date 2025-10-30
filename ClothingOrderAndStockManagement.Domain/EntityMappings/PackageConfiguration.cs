using ClothingOrderAndStockManagement.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClothingOrderAndStockManagement.Domain.EntityMappings
{
    public class PackageConfiguration : IEntityTypeConfiguration<Package>
    {
        public void Configure(EntityTypeBuilder<Package> entity)
        {
            entity.HasKey(e => e.PackagesId).HasName("PK__Packages__BAFF4A30CFB5A712");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.PackageName).HasMaxLength(100);
            entity.Property(p => p.Price).HasPrecision(10, 2);
        }
    }
}
