using ClothingOrderAndStockManagement.Domain.Entities.Customers;

namespace ClothingOrderAndStockManagement.Domain.Entities.Orders;

public class OrderRecord
{
    public int OrderRecordsId { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDatetime { get; set; }
    public string OrderStatus { get; set; }
    public int? PaymentRecordsId { get; set; }
    // Initialize collections to prevent null reference exceptions
    public virtual ICollection<OrderPackage> OrderPackages { get; set; } = new List<OrderPackage>();
    public virtual ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();
    public virtual ICollection<ReturnLog> ReturnLogs { get; set; } = new List<ReturnLog>();
}
