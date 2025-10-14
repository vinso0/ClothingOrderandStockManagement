using ClothingOrderAndStockManagement.Domain.Entities.Orders;

namespace ClothingOrderAndStockManagement.Domain.Entities.Products;

public partial class Package
{
    public int PackagesId { get; set; }

    public string PackageName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<OrderPackage> OrderPackages { get; set; } = new List<OrderPackage>();

    public virtual ICollection<PackageItem> PackageItems { get; set; } = new List<PackageItem>();
}
