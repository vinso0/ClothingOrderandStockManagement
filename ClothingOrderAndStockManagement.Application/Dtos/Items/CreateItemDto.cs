namespace ClothingOrderAndStockManagement.Application.Dtos.Items
{
    public class CreateItemDto
    {
        public int ItemCategoryId { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public int Quantity { get; set; }
    }
}
