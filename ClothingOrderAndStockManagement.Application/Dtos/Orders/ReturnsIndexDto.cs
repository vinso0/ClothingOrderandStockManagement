using ClothingOrderAndStockManagement.Application.Helpers;

namespace ClothingOrderAndStockManagement.Application.Dtos.Orders
{
    public class ReturnsIndexViewModel
    {
        // Use OrderRecordDto for display (same as Staff function)
        public PaginatedList<OrderRecordDto> CompletedOrders { get; set; } = new PaginatedList<OrderRecordDto>(new List<OrderRecordDto>(), 0, 1, 10);

        // Keep ReturnLogDto for return history display
        public PaginatedList<ReturnLogDto> ReturnHistory { get; set; } = new PaginatedList<ReturnLogDto>(new List<ReturnLogDto>(), 0, 1, 10);
    }
}
