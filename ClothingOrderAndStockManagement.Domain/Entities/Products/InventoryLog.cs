namespace ClothingOrderAndStockManagement.Domain.Entities.Products;

public partial class InventoryLog
{
    public int LogId { get; set; }

    public int? ItemId { get; set; }

    public int? PackageItemId { get; set; }

    public string ChangeType { get; set; } = null!;

    public int QuantityChanged { get; set; }

    public DateOnly LogDate { get; set; }

    public TimeOnly LogTime { get; set; }

    public string UserId { get; set; } = null!;

    public virtual Item? Item { get; set; }

    public virtual PackageItem? PackageItem { get; set; }
}
