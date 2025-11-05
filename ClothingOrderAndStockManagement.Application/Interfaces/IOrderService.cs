using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Helpers;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace ClothingOrderAndStockManagement.Domain.Interfaces
{
    public interface IOrderService
    {
        Task<Result<IEnumerable<OrderRecordDto>>> GetAllAsync();
        Task<Result<OrderRecordDto>> GetByIdAsync(int id);
        Task<Result<int>> CreateAsync(CreateOrderDto dto);
        Task<Result> UpdateAsync(OrderRecordDto orderDto);
        Task<Result> DeleteAsync(int id);
        Task<Result> AddPaymentAsync(AddPaymentDto dto, IFormFile? proof1, IFormFile? proof2);
        Task<Result<int>> CreateWithPaymentAsync(CreateOrderDto dto, IFormFile? proof1, IFormFile? proof2);
        Task<Result<IEnumerable<OrderRecordDto>>> GetOrdersForSortingAsync();
        Task<Result<PaginatedList<OrderRecordDto>>> GetOrdersForReturnsAsync(
            string? searchString = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            int pageIndex = 1,
            int pageSize = 10);

        Task<Result<PaginatedList<OrderRecordDto>>> GetFilteredOrdersAsync(string? status, int pageIndex, int pageSize = 5);
        Task<Result<bool>> IsValidOrderStatusAsync(string status);
        Task<Result> UpdateOrderStatusAsync(int orderId, string newStatus);
        Task<Result<string[]>> GetValidOrderStatusesAsync();
        Task<Result<PaginatedList<OrderRecordDto>>> GetStaffOrdersAsync(int pageIndex, int pageSize);
        Task<Result> CompleteOrderAsync(int orderId);
    }
}
