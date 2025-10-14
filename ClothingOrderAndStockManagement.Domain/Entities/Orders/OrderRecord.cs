using ClothingOrderAndStockManagement.Domain.Entities.Report;

namespace ClothingOrderAndStockManagement.Domain.Entities.Orders;

public partial class OrderRecord
{
    public int OrderRecordsId { get; set; }

    public int CustomerId { get; set; }

    public DateTime OrderDatetime { get; set; }

    public string OrderStatus { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public int? PaymentRecordsId { get; set; }

    public virtual ICollection<OrderPackage> OrderPackages { get; set; } = new List<OrderPackage>();

    public virtual ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();

    public virtual ICollection<ReturnLog> ReturnLogs { get; set; } = new List<ReturnLog>();

    public virtual ICollection<SalesReport> SalesReports { get; set; } = new List<SalesReport>();
}
