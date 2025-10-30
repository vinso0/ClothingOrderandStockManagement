using ClothingOrderAndStockManagement.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClothingOrderAndStockManagement.Domain.EntityMappings
{
    public class OrderRecordConfiguration : IEntityTypeConfiguration<OrderRecord>
    {
        public void Configure(EntityTypeBuilder<OrderRecord> entity)
        {
            entity.HasKey(e => e.OrderRecordsId).HasName("PK__OrderRec__344164E7BC4CF5E5");

            entity.Property(e => e.OrderDatetime).HasColumnType("datetime");
            entity.Property(e => e.OrderStatus).HasMaxLength(50);
        }
    }
}
