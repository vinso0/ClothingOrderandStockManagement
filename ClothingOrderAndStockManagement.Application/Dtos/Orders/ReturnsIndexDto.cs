using ClothingOrderAndStockManagement.Application.Helpers;

namespace ClothingOrderAndStockManagement.Application.Dtos.Orders
{
    public class ReturnsIndexDto
    {
        public PaginatedList<CompletedOrderDto> Completed { get; set; } = new PaginatedList<CompletedOrderDto>(new List<CompletedOrderDto>(), 0, 1, 10);
        public PaginatedList<ReturnLogDto> History { get; set; } = new PaginatedList<ReturnLogDto>(new List<ReturnLogDto>(), 0, 1, 10);
    }
}
