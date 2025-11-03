namespace ClothingOrderAndStockManagement.Application.Dtos.Packages
{
    public class CreatePackageDto
    {
        public string PackageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public List<CreatePackageItemDto> PackageItems { get; set; } = new();
    }

    public class CreatePackageItemDto
    {
        public int ItemId { get; set; }
        public int ItemQuantity { get; set; }
    }
}
