using ClothingOrderAndStockManagement.Application.Dtos.Orders;

namespace ClothingOrderAndStockManagement.Application.Dtos.Orders
{
    public class OrderRecordDto
    {
        public int OrderRecordsId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDatetime { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public List<OrderPackageDto> OrderPackages { get; set; } = new();
        public List<PaymentRecordDto> PaymentRecords { get; set; } = new();

        // Computed properties
        public decimal TotalAmount => OrderPackages.Sum(p => p.PriceAtPurchase * p.Quantity);
        public decimal TotalPaid => PaymentRecords.Sum(p => p.Amount);
        public decimal RemainingBalance => TotalAmount - TotalPaid;
    }
}