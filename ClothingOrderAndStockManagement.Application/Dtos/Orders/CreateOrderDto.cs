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
    }

    public class OrderPackageItemDto
    {
        public int PackagesId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }
}