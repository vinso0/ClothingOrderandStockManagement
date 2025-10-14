namespace ClothingOrderAndStockManagement.Domain.Entities.Products;

public partial class Item
{
    public int ItemId { get; set; }

    public int ItemCategoryId { get; set; }

    public string? Size { get; set; }

    public string? Color { get; set; }

    public int Quantity { get; set; }

    public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();

    public virtual ItemCategory ItemCategory { get; set; } = null!;

    public virtual ICollection<PackageItem> PackageItems { get; set; } = new List<PackageItem>();
}
