namespace ClothingOrderAndStockManagement.Application.Dtos.Orders
{
    public class OrderPackageDto
    {
        public int OrderPackagesId { get; set; }
        public int PackagesId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public string PackageName { get; set; } = string.Empty;
    }
}
