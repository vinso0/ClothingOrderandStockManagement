using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Helpers;
using FluentResults;

namespace ClothingOrderAndStockManagement.Domain.Interfaces
{
    public interface IReturnService
    {
        Task<Result<PaginatedList<CompletedOrderDto>>> GetCompletedOrdersAsync(
            string searchString,
            DateOnly? fromDate,
            DateOnly? toDate,
            int pageIndex,
            int pageSize);

        Task<Result<ReturnLogDto>> ProcessReturnAsync(ReturnRequestDto returnRequest);

        Task<Result<PaginatedList<ReturnLogDto>>> GetReturnsAsync(
            string searchString,
            DateOnly? fromDate,
            DateOnly? toDate,
            int pageIndex,
            int pageSize);
    }
}
