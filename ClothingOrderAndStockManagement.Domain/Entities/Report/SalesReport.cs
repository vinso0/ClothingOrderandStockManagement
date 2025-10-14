using ClothingOrderAndStockManagement.Domain.Entities.Orders;

namespace ClothingOrderAndStockManagement.Domain.Entities.Report;

public partial class SalesReport
{
    public int SalesReportId { get; set; }

    public DateOnly ReportDate { get; set; }

    public decimal TotalSales { get; set; }

    public int OrderRecordsId { get; set; }

    public string UserId { get; set; } = null!;

    public virtual OrderRecord OrderRecords { get; set; } = null!;
}
