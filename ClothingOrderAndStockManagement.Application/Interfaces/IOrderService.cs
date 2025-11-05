using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Helpers;
using FluentResults;
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
        Task<bool> AddPaymentAsync(AddPaymentDto dto, IFormFile? proof1, IFormFile? proof2);
        Task<int> CreateWithPaymentAsync(CreateOrderDto dto, IFormFile? proof1, IFormFile? proof2);
        Task<IEnumerable<OrderRecordDto>> GetOrdersForSortingAsync();
        Task<Result<PaginatedList<OrderRecordDto>>> GetOrdersForReturnsAsync(
        string? searchString = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        int pageIndex = 1,
        int pageSize = 10);
    }
}