namespace ClothingOrderAndStockManagement.Application.Dtos.Packages
{
    public class PackageDetailDto
{
    public int PackagesId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public List<PackageItemDto> PackageItems { get; set; } = new();
}

public class PackageItemDto
{
    public int PackageItemId { get; set; }
    public int ItemId { get; set; }
    public int ItemQuantity { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Color { get; set; }
}
}
