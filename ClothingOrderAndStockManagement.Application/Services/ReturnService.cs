using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Orders;
using FluentResults;
using Microsoft.EntityFrameworkCore;

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
                var baseQuery = _returnRepository.GetCompletedOrdersQuery();

                // Project to DTO with customer info via ReturnLogs navigation
                var dtoQuery = baseQuery
                    .Select(o => new CompletedOrderDto
                    {
                        OrderRecordsId = o.OrderRecordsId,
                        OrderPackagesId = o.OrderPackages.Select(p => p.OrderPackagesId).FirstOrDefault(),
                        CustomerId = o.CustomerId,
                        // Get customer name via ReturnLogs -> CustomerInfo navigation
                        CustomerName = o.ReturnLogs
                            .Select(rl => rl.CustomerInfo.CustomerName)
                            .FirstOrDefault() ?? $"Customer {o.CustomerId}",
                        CustomerEmail = o.ReturnLogs
                            .Select(rl => rl.CustomerInfo.ContactNumber)
                            .FirstOrDefault() ?? "",
                        OrderDate = DateOnly.FromDateTime(o.OrderDatetime),
                        TotalAmount = o.OrderPackages.Sum(op => op.PriceAtPurchase * op.Quantity),
                        Status = o.OrderStatus,
                        ItemCount = o.OrderPackages.Sum(op => op.Quantity)
                    });

                // Apply filters on the DTO query
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    dtoQuery = dtoQuery.Where(d =>
                        d.CustomerName.Contains(searchString) ||
                        d.CustomerEmail.Contains(searchString));
                }

                if (fromDate.HasValue)
                {
                    dtoQuery = dtoQuery.Where(d => d.OrderDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    dtoQuery = dtoQuery.Where(d => d.OrderDate <= toDate.Value);
                }

                var paginatedList = await PaginatedList<CompletedOrderDto>.CreateAsync(dtoQuery, pageIndex, pageSize);
                return Result.Ok(paginatedList);
            }
            catch (Exception ex)
            {
                return Result.Fail<PaginatedList<CompletedOrderDto>>(ex.Message);
            }
        }

        public async Task<Result<ReturnLogDto>> ProcessReturnAsync(ReturnRequestDto returnRequest)
        {
            try
            {
                // Validate
                var validationResult = ValidateReturnRequest(returnRequest);
                if (validationResult.IsFailed)
                    return validationResult;

                var returnLog = new ReturnLog
                {
                    OrderRecordsId = returnRequest.OrderRecordsId,
                    OrderPackagesId = returnRequest.OrderPackagesId,
                    ReturnDate = DateOnly.FromDateTime(DateTime.Now),
                    Reason = returnRequest.Reason
                };

                // Add return log
                await _returnRepository.AddReturnLogAsync(returnLog);

                // Update order status
                var updateResult = await _returnRepository.UpdateOrderStatusToReturnedAsync(returnRequest.OrderRecordsId);
                if (!updateResult)
                {
                    return Result.Fail<ReturnLogDto>("Failed to update order status");
                }

                // Restock if requested
                if (returnRequest.RestockItems)
                {
                    var restockResult = await _returnRepository.RestockItemsAsync(returnRequest.OrderPackagesId);
                    if (!restockResult)
                    {
                        return Result.Fail<ReturnLogDto>("Failed to restock items");
                    }
                }

                await _returnRepository.SaveChangesAsync();

                var returnDto = new ReturnLogDto
                {
                    OrderRecordsId = returnLog.OrderRecordsId,
                    OrderPackagesId = returnLog.OrderPackagesId,
                    ReturnDate = returnLog.ReturnDate,
                    Reason = returnLog.Reason,
                    RestockItems = returnRequest.RestockItems
                };

                return Result.Ok(returnDto);
            }
            catch (Exception ex)
            {
                return Result.Fail<ReturnLogDto>(ex.Message);
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
                var query = _returnRepository.GetReturnsQuery();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    query = query.Where(r =>
                        r.CustomerInfo.CustomerName.Contains(searchString) ||
                        r.Reason!.Contains(searchString));
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(r => r.ReturnDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(r => r.ReturnDate <= toDate.Value);
                }

                // Project to DTO
                var dtoQuery = query.Select(r => new ReturnLogDto
                {
                    ReturnLogsId = r.ReturnLogsId,
                    OrderRecordsId = r.OrderRecordsId,
                    OrderPackagesId = r.OrderPackagesId,
                    CustomerId = r.CustomerId,
                    ReturnDate = r.ReturnDate,
                    Reason = r.Reason!,
                    CustomerName = r.CustomerInfo.CustomerName,
                    CustomerEmail = r.CustomerInfo.ContactNumber,
                    OrderTotal = r.OrderRecords.OrderPackages.Sum(op => op.PriceAtPurchase * op.Quantity),
                    OrderDate = DateOnly.FromDateTime(r.OrderRecords.OrderDatetime),
                    OrderStatus = r.OrderRecords.OrderStatus
                });

                var paginatedList = await PaginatedList<ReturnLogDto>.CreateAsync(dtoQuery, pageIndex, pageSize);
                return Result.Ok(paginatedList);
            }
            catch (Exception ex)
            {
                return Result.Fail<PaginatedList<ReturnLogDto>>(ex.Message);
            }
        }

        private Result ValidateReturnRequest(ReturnRequestDto returnRequest)
        {
            if (returnRequest.OrderRecordsId <= 0)
                return Result.Fail("Order ID is required");

            if (returnRequest.OrderPackagesId <= 0)
                return Result.Fail("Order Package ID is required");

            if (string.IsNullOrWhiteSpace(returnRequest.Reason))
                return Result.Fail("Return reason is required");

            if (returnRequest.Reason.Length > 500)
                return Result.Fail("Return reason cannot exceed 500 characters");

            return Result.Ok();
        }
    }
}
