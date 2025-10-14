using ClothingOrderAndStockManagement.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClothingOrderAndStockManagement.Infrastructure.Data.EntityMappings
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> entity)
        {
            entity.ToTable("CustomerInfo");

            entity.HasKey(e => e.CustomerId);

            entity.Property(e => e.CustomerName)
                .HasMaxLength(100)
                .IsUnicode(true);

            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(true);

            entity.Property(e => e.ZipCode)
                .HasMaxLength(10)
                .IsUnicode(true);

            entity.Property(e => e.ContactNumber)
                .HasMaxLength(20)
                .IsUnicode(true);
        }
    }
}
