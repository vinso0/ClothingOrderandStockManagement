namespace ClothingOrderAndStockManagement.Domain.Entities.Products;

public partial class ItemCategory
{
    public int ItemCategoryId { get; set; }

    public string ItemCategoryType { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
