namespace ClothingOrderAndStockManagement.Application.Dtos.Packages
{
    public class PackageItemDto
    {
        public int PackageItemId { get; set; }
        public int ItemId { get; set; }
        public int ItemQuantity { get; set; }
        public string? ItemName { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
    }
}
