namespace ClothingOrderAndStockManagement.Application.Dtos.Customers
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }
}
