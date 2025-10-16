using ClothingOrderAndStockManagement.Application.Dtos.Orders;

namespace ClothingOrderAndStockManagement.Application.Dtos.Orders
{
    public class OrderRecordDto
    {
        public int OrderRecordsId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDatetime { get; set; }
        public string OrderStatus { get; set; }
        public string UserId { get; set; }
        public List<OrderPackageDto> OrderPackages { get; set; } = new();
        public List<PaymentRecordDto> PaymentRecords { get; set; } = new();
    }
}