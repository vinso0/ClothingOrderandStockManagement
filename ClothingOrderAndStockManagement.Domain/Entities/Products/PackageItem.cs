namespace ClothingOrderAndStockManagement.Domain.Entities.Products;

public partial class PackageItem
{
    public int PackageItemId { get; set; }

    public int ItemId { get; set; }

    public int ItemQuantity { get; set; }

    public int PackagesId { get; set; }

    public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();

    public virtual Item Item { get; set; } = null!;

    public virtual Package Packages { get; set; } = null!;
}
