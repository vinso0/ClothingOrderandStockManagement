using ClothingOrderAndStockManagement.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClothingOrderAndStockManagement.Domain.EntityMappings
{
    public class ItemCategoryConfiguration : IEntityTypeConfiguration<ItemCategory>
    {
        public void Configure(EntityTypeBuilder<ItemCategory> entity)
        {
            entity.HasKey(e => e.ItemCategoryId).HasName("PK__ItemCate__C24A29255FC49F7D");

            entity.ToTable("ItemCategory");

            entity.Property(e => e.ItemCategoryType).HasMaxLength(100);
        }
    }
}
