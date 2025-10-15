namespace ClothingOrderAndStockManagement.Domain.Entities.Customers;

public partial class CustomerInfo
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string ContactNumber { get; set; } = null!;

    public string ZipCode { get; set; } = null!;
}
