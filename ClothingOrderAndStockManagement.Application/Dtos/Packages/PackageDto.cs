using ClothingOrderAndStockManagement.Application.Dtos.Items;

namespace ClothingOrderAndStockManagement.Application.Dtos.Packages
{
    public class PackageDto
    {
        public int PackagesId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public List<PackageItemDetailDto> PackageItems { get; set; } = new();
    }

    public class PackageItemDetailDto
    {
        public int PackageItemId { get; set; }
        public int ItemId { get; set; }
        public int ItemQuantity { get; set; }
        public ItemDto Item { get; set; } = new();
    }
}
