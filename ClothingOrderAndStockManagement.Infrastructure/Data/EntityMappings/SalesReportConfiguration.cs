using ClothingOrderAndStockManagement.Domain.Entities.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClothingOrderAndStockManagement.Infrastructure.Data.EntityMappings
{
    public class SalesReportConfiguration : IEntityTypeConfiguration<SalesReport>
    {
        public void Configure(EntityTypeBuilder<SalesReport> entity)
        {
            entity.HasKey(e => e.SalesReportId).HasName("PK__SalesRep__ED7CE8865C2BE599");

            entity.ToTable("SalesReport");

            entity.Property(e => e.TotalSales).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.OrderRecords).WithMany(p => p.SalesReports)
                .HasForeignKey(d => d.OrderRecordsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesReport_Order");
        }
    }
}
