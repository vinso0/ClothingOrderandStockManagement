namespace ClothingOrderAndStockManagement.Application.Dtos.Packages
{
    public class PackageDetailDto
    {
        public int PackagesId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
        public List<PackageItemDto> PackageItems { get; set; } = new();
    }
}
