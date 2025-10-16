namespace ClothingOrderAndStockManagement.Application.Dtos.Orders
{
    public class CreateOrderDto
    {
        public int CustomerId { get; set; }

        public DateTime OrderDatetime { get; set; } = DateTime.Now;

        public string OrderStatus { get; set; } = "Pending";

        public string? UserId { get; set; }

        // Optional: include initial packages if your UI supports it
        //public List<OrderPackageDto>? OrderPackages { get; set; }
    }
}