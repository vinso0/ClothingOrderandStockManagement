using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using Microsoft.AspNetCore.Http;

namespace ClothingOrderAndStockManagement.Domain.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderRecordDto>> GetAllAsync();
        Task<OrderRecordDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateOrderDto dto);
        Task<bool> UpdateAsync(OrderRecordDto orderDto);
        Task<bool> DeleteAsync(int id);

        // Payment management
        Task<bool> AddPaymentAsync(AddPaymentDto dto, IFormFile? proof1, IFormFile? proof2);

        // Legacy - kept for backward compatibility
        Task<int> CreateWithPaymentAsync(CreateOrderDto dto, IFormFile? proof1, IFormFile? proof2);
    }
}