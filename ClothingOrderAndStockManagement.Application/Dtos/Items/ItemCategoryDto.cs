namespace ClothingOrderAndStockManagement.Application.Dtos.Items
{
    public class ItemCategoryDto
    {
        public int ItemCategoryId { get; set; }
        public string ItemCategoryType { get; set; } = string.Empty;
        public int ItemsCount { get; set; } // Calculated field for UI display
    }

    public class CreateItemCategoryDto
    {
        public string ItemCategoryType { get; set; } = string.Empty;
    }

    public class UpdateItemCategoryDto
    {
        public int ItemCategoryId { get; set; }
        public string ItemCategoryType { get; set; } = string.Empty;
    }
}
