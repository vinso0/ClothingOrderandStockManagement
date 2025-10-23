namespace ClothingOrderAndStockManagement.Application.Dtos.Orders
{
    public class UpdateOrderDto
    {
        public int OrderRecordsId { get; set; }

        public int CustomerId { get; set; }

        public DateTime OrderDatetime { get; set; }

        public string OrderStatus { get; set; } = string.Empty;

        // Optional: Allow updating order packages if needed
        //public List<OrderPackageDto>? OrderPackages { get; set; }
    }
}