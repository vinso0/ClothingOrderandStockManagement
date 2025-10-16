using ClothingOrderAndStockManagement.Domain.Entities.Customers;

namespace ClothingOrderAndStockManagement.Domain.Entities.Orders;

public class OrderRecord
{
    public int OrderRecordsId { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDatetime { get; set; }
    public string OrderStatus { get; set; }
    public string UserId { get; set; }
    public int? PaymentRecordsId { get; set; }
    public virtual ICollection<OrderPackage> OrderPackages { get; set; }
    public virtual ICollection<PaymentRecord> PaymentRecords { get; set; }
    public virtual ICollection<ReturnLog> ReturnLogs { get; set; }
}
