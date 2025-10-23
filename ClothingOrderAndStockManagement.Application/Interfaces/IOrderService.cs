using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using Microsoft.AspNetCore.Http;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderRecordDto>> GetAllAsync();
        Task<OrderRecordDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(OrderRecordDto orderDto);
        Task<bool> UpdateAsync(OrderRecordDto orderDto);
        Task<bool> DeleteAsync(int id);

        // Use-case method that handles files, mapping, and status rules
        Task<int> CreateWithPaymentAsync(CreateOrderDto dto, IFormFile? proof1, IFormFile? proof2);
    }
}
