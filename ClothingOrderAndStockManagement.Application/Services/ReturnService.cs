using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Application.Repositories;
using ClothingOrderAndStockManagement.Domain.Entities.Orders;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using FluentResults;

namespace ClothingOrderAndStockManagement.Application.Services
{
    public class ReturnService : IReturnService
    {
        private readonly IReturnRepository _returnRepository;

        public ReturnService(IReturnRepository returnRepository)
        {
            _returnRepository = returnRepository;
        }

        public async Task<Result<PaginatedList<CompletedOrderDto>>> GetCompletedOrdersAsync(
            string searchString,
            DateOnly? fromDate,
            DateOnly? toDate,
            int pageIndex,
            int pageSize)
        {
            try
            {
                var completedOrders = await _returnRepository.GetCompletedOrdersAsync(
                    searchString, fromDate, toDate, pageIndex, pageSize);
                return Result<PaginatedList<CompletedOrderDto>>.Success(completedOrders);
            }
            catch (Exception ex)
            {
                return Result<PaginatedList<CompletedOrderDto>>.Failure(
                    new Error("GetCompletedOrders.Error", ex.Message));
            }
        }

        public async Task<Result<ReturnLogDto>> ProcessReturnAsync(ReturnRequestDto returnRequest)
        {
            try
            {
                // Manual validation
                var validationErrors = ValidateReturnRequest(returnRequest);
                if (validationErrors.Any())
                {
                    return Result<ReturnLogDto>.Failure(validationErrors.ToArray());
                }

                var returnLog = new ReturnLog
                {
                    OrderRecordsId = returnRequest.OrderRecordsId,
                    OrderPackagesId = returnRequest.OrderPackagesId,
                    ReturnDate = DateOnly.FromDateTime(DateTime.Now),
                    Reason = returnRequest.Reason
                };

                var addResult = await _returnRepository.AddReturnLogAsync(returnLog);
                if (!addResult)
                {
                    return Result<ReturnLogDto>.Failure(
                        new Error("ProcessReturn.AddFailed", "Failed to add return log"));
                }

                var updateResult = await _returnRepository.UpdateOrderStatusToReturnedAsync(returnRequest.OrderRecordsId);
                if (!updateResult)
                {
                    return Result<ReturnLogDto>.Failure(
                        new Error("ProcessReturn.UpdateFailed", "Failed to update order status"));
                }

                if (returnRequest.RestockItems)
                {
                    var restockResult = await _returnRepository.RestockItemsAsync(returnRequest.OrderPackagesId);
                    if (!restockResult)
                    {
                        return Result<ReturnLogDto>.Failure(
                            new Error("ProcessReturn.RestockFailed", "Failed to restock items"));
                    }
                }

                var returnDto = new ReturnLogDto
                {
                    OrderRecordsId = returnLog.OrderRecordsId,
                    OrderPackagesId = returnLog.OrderPackagesId,
                    ReturnDate = returnLog.ReturnDate,
                    Reason = returnLog.Reason,
                    RestockItems = returnRequest.RestockItems
                };

                return Result<ReturnLogDto>.Success(returnDto);
            }
            catch (Exception ex)
            {
                return Result<ReturnLogDto>.Failure(
                    new Error("ProcessReturn.Error", ex.Message));
            }
        }

        public async Task<Result<PaginatedList<ReturnLogDto>>> GetReturnsAsync(
            string searchString,
            DateOnly? fromDate,
            DateOnly? toDate,
            int pageIndex,
            int pageSize)
        {
            try
            {
                var returns = await _returnRepository.GetReturnsAsync(
                    searchString, fromDate, toDate, pageIndex, pageSize);
                return Result<PaginatedList<ReturnLogDto>>.Success(returns);
            }
            catch (Exception ex)
            {
                return Result<PaginatedList<ReturnLogDto>>.Failure(
                    new Error("GetReturns.Error", ex.Message));
            }
        }

        private List<Error> ValidateReturnRequest(ReturnRequestDto returnRequest)
        {
            var errors = new List<Error>();

            if (returnRequest.OrderRecordsId <= 0)
            {
                errors.Add(new Error("Validation.OrderRecordsId", "Order ID is required"));
            }

            if (returnRequest.OrderPackagesId <= 0)
            {
                errors.Add(new Error("Validation.OrderPackagesId", "Order Package ID is required"));
            }

            if (string.IsNullOrWhiteSpace(returnRequest.Reason))
            {
                errors.Add(new Error("Validation.Reason", "Return reason is required"));
            }
            else if (returnRequest.Reason.Length > 500)
            {
                errors.Add(new Error("Validation.Reason", "Return reason cannot exceed 500 characters"));
            }

            return errors;
        }
    }
}
