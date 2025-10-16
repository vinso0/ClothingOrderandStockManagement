using ClothingOrderAndStockManagement.Application.Dtos.Orders;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderRecordDto>> GetAllAsync();
        Task<OrderRecordDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(OrderRecordDto orderDto);
        Task<bool> UpdateAsync(OrderRecordDto orderDto);
        Task<bool> DeleteAsync(int id);
    }
}
