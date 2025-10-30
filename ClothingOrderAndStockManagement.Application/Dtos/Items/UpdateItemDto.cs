namespace ClothingOrderAndStockManagement.Application.Dtos.Items
{
    public class UpdateItemDto
    {
        public int ItemId { get; set; }
        public int ItemCategoryId { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public int Quantity { get; set; }
    }
}
