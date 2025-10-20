namespace ClothingOrderAndStockManagement.Application.Dtos.Orders
{
    public class CreateOrderDto
    {
        public int CustomerId { get; set; }
        public DateTime OrderDatetime { get; set; } = DateTime.Now;
        public string OrderStatus { get; set; } = "Pending Payment";
        public string? UserId { get; set; }

        // Order packages with quantity
        public List<OrderPackageItemDto> OrderPackages { get; set; } = new();

        // Initial payment if any
        public PaymentDto? InitialPayment { get; set; }
    }

    public class OrderPackageItemDto
    {
        public int PackagesId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }

    public class PaymentDto
    {
        public decimal Amount { get; set; }
        public string? ProofUrl { get; set; }
        public string? ProofUrl2 { get; set; }
        public string PaymentStatus { get; set; } = "Down Payment";
    }
}