using ClothingOrderAndStockManagement.Domain.Entities.Products;

namespace ClothingOrderAndStockManagement.Domain.Entities.Orders;

public partial class OrderPackage
{
    public int OrderPackagesId { get; set; }

    public int OrderRecordsId { get; set; }

    public int PackagesId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceAtPurchase { get; set; }

    public virtual OrderRecord OrderRecords { get; set; } = null!;

    public virtual Package Packages { get; set; } = null!;
}
