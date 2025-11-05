using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Orders;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using ClothingOrderAndStockManagement.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IInventoryService _inventoryService;

        private static readonly string[] ValidOrderStatuses =
        {
            "Awaiting Payment",
            "Partially Paid",
            "Fully Paid",
            "Completed",
            "Returned",
            "Cancelled"
        };

        public OrderService(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IInventoryService inventoryService)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _inventoryService = inventoryService;
        }

        public async Task<Result<PaginatedList<OrderRecordDto>>> GetFilteredOrdersAsync(string? status, int pageIndex, int pageSize = 5)
        {
            try
            {
                var ordersResult = await GetAllAsync();
                if (ordersResult.IsFailed)
                    return Result.Fail<PaginatedList<OrderRecordDto>>(ordersResult.Errors);

                var orders = ordersResult.Value;

                var normalized = string.IsNullOrWhiteSpace(status) ? "" : status.Trim();

                if (!string.IsNullOrEmpty(normalized) && IsValidOrderStatus(normalized))
                {
                    orders = orders.Where(o => string.Equals(o.OrderStatus, normalized, StringComparison.Ordinal));
                }

                var sortedOrders = orders.OrderByDescending(o => o.OrderDatetime).ToList();

                var totalCount = sortedOrders.Count;
                var pagedOrders = sortedOrders
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var paginatedList = new PaginatedList<OrderRecordDto>(pagedOrders, totalCount, pageIndex, pageSize);
                return Result.Ok(paginatedList);
            }
            catch (Exception ex)
            {
                return Result.Fail<PaginatedList<OrderRecordDto>>(ex.Message);
            }
        }

        public async Task<Result<bool>> IsValidOrderStatusAsync(string status)
        {
            try
            {
                var isValid = IsValidOrderStatus(status);
                return await Task.FromResult(Result.Ok(isValid));
            }
            catch (Exception ex)
            {
                return Result.Fail<bool>(ex.Message);
            }
        }

        public async Task<Result> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            try
            {
                if (!IsValidOrderStatus(newStatus))
                {
                    return Result.Fail("Invalid order status.");
                }

                var orderResult = await GetByIdAsync(orderId);
                if (orderResult.IsFailed)
                    return Result.Fail(orderResult.Errors);

                var order = orderResult.Value;
                order.OrderStatus = newStatus;

                return await UpdateAsync(order);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Result<string[]>> GetValidOrderStatusesAsync()
        {
            try
            {
                return await Task.FromResult(Result.Ok(ValidOrderStatuses));
            }
            catch (Exception ex)
            {
                return Result.Fail<string[]>(ex.Message);
            }
        }

        private static bool IsValidOrderStatus(string status)
        {
            return ValidOrderStatuses.Any(s => string.Equals(s, status, StringComparison.Ordinal));
        }

        public async Task<Result<IEnumerable<OrderRecordDto>>> GetAllAsync()
        {
            try
            {
                var orders = await _orderRepository.Query()
                    .Include(o => o.OrderPackages).ThenInclude(op => op.Packages)
                    .Include(o => o.PaymentRecords)
                    .ToListAsync();

                var customers = await _customerRepository.GetAllAsync();
                var orderDtos = orders.Select(o => MapToDto(o, customers));

                return Result.Ok(orderDtos);
            }
            catch (Exception ex)
            {
                return Result.Fail<IEnumerable<OrderRecordDto>>(ex.Message);
            }
        }

        public async Task<Result<OrderRecordDto>> GetByIdAsync(int id)
        {
            try
            {
                var order = await _orderRepository.Query()
                    .Include(o => o.OrderPackages).ThenInclude(p => p.Packages)
                    .Include(o => o.PaymentRecords)
                    .FirstOrDefaultAsync(o => o.OrderRecordsId == id);

                if (order == null)
                    return Result.Fail<OrderRecordDto>("Order not found.");

                var customers = await _customerRepository.GetAllAsync();
                var orderDto = MapToDto(order, customers);

                return Result.Ok(orderDto);
            }
            catch (Exception ex)
            {
                return Result.Fail<OrderRecordDto>(ex.Message);
            }
        }

        public async Task<Result<int>> CreateAsync(CreateOrderDto dto)
        {
            try
            {
                if (dto.OrderPackages == null || dto.OrderPackages.Count == 0)
                    return Result.Fail<int>("Add at least one package.");

                foreach (var p in dto.OrderPackages)
                {
                    if (p.PackagesId <= 0 || p.Quantity <= 0)
                        return Result.Fail<int>("Package and positive quantity are required.");
                }

                // Validate availability
                foreach (var p in dto.OrderPackages)
                {
                    var ok = await _inventoryService.ValidatePackageAvailabilityAsync(p.PackagesId, p.Quantity);
                    if (!ok)
                        return Result.Fail<int>("Insufficient package availability.");
                }

                // Reserve
                var reserved = new List<(int packageId, int qty)>();
                try
                {
                    foreach (var p in dto.OrderPackages)
                    {
                        var res = await _inventoryService.ReservePackageQuantityAsync(p.PackagesId, p.Quantity);
                        if (res.IsFailed)
                            return Result.Fail<int>(res.Errors[0].Message);
                        reserved.Add((p.PackagesId, p.Quantity));
                    }

                    var order = new OrderRecord
                    {
                        CustomerId = dto.CustomerId,
                        OrderDatetime = dto.OrderDatetime,
                        OrderStatus = "Awaiting Payment",
                        OrderPackages = dto.OrderPackages.Select(p => new OrderPackage
                        {
                            PackagesId = p.PackagesId,
                            Quantity = p.Quantity,
                            PriceAtPurchase = p.PriceAtPurchase
                        }).ToList()
                    };

                    await _orderRepository.AddAsync(order);
                    await _orderRepository.SaveChangesAsync();

                    return Result.Ok(order.OrderRecordsId);
                }
                catch
                {
                    foreach (var (pid, q) in reserved)
                        await _inventoryService.ReleasePackageQuantityAsync(pid, q);
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Result.Fail<int>(ex.Message);
            }
        }

        public async Task<Result> UpdateAsync(OrderRecordDto dto)
        {
            try
            {
                var existing = await _orderRepository.Query()
                    .Include(o => o.OrderPackages)
                    .Include(o => o.PaymentRecords)
                    .FirstOrDefaultAsync(o => o.OrderRecordsId == dto.OrderRecordsId);

                if (existing == null)
                    return Result.Fail("Order not found.");

                var wasCancelled = existing.OrderStatus == "Cancelled";
                var willBeCancelled = dto.OrderStatus == "Cancelled";

                if (!wasCancelled && willBeCancelled)
                {
                    foreach (var p in existing.OrderPackages)
                        await _inventoryService.ReleasePackageQuantityAsync(p.PackagesId, p.Quantity);
                }

                existing.OrderStatus = dto.OrderStatus;

                await _orderRepository.UpdateAsync(existing);
                await _orderRepository.SaveChangesAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Result> DeleteAsync(int id)
        {
            try
            {
                var existing = await _orderRepository.Query()
                    .Include(o => o.OrderPackages)
                    .FirstOrDefaultAsync(o => o.OrderRecordsId == id);

                if (existing != null)
                {
                    foreach (var p in existing.OrderPackages)
                        await _inventoryService.ReleasePackageQuantityAsync(p.PackagesId, p.Quantity);
                }

                await _orderRepository.DeleteAsync(id);
                await _orderRepository.SaveChangesAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Result> AddPaymentAsync(AddPaymentDto dto, IFormFile? proof1, IFormFile? proof2)
        {
            try
            {
                var order = await _orderRepository.Query()
                    .Include(o => o.OrderPackages)
                    .Include(o => o.PaymentRecords)
                    .FirstOrDefaultAsync(o => o.OrderRecordsId == dto.OrderRecordsId);

                if (order == null)
                    return Result.Fail("Order not found.");

                string? url1 = null;
                if (proof1 != null)
                {
                    var proofResult = await SavePaymentProofAsync(proof1);
                    if (proofResult.IsFailed)
                        return Result.Fail(proofResult.Errors);
                    url1 = proofResult.Value;
                }

                var totalAmount = order.OrderPackages.Sum(p => p.PriceAtPurchase * p.Quantity);
                var currentTotalPaid = order.PaymentRecords.Sum(p => p.Amount);
                var newTotalPaid = currentTotalPaid + dto.Amount;

                string paymentStatus = dto.PaymentStatus;
                if (newTotalPaid >= totalAmount) paymentStatus = "Full Payment";

                order.PaymentRecords.Add(new Domain.Entities.Orders.PaymentRecord
                {
                    OrderRecordsId = dto.OrderRecordsId,
                    CustomerId = order.CustomerId,
                    Amount = dto.Amount,
                    ProofUrl = url1,
                    PaymentStatus = paymentStatus,
                    PaymentDate = DateTime.Now
                });

                if (newTotalPaid >= totalAmount) order.OrderStatus = "Fully Paid";
                else if (newTotalPaid > 0) order.OrderStatus = "Partially Paid";
                else order.OrderStatus = "Awaiting Payment";

                await _orderRepository.UpdateAsync(order);
                await _orderRepository.SaveChangesAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Result<int>> CreateWithPaymentAsync(CreateOrderDto dto, IFormFile? proof1, IFormFile? proof2)
        {
            try
            {
                return await CreateAsync(dto);
            }
            catch (Exception ex)
            {
                return Result.Fail<int>(ex.Message);
            }
        }

        public async Task<Result<IEnumerable<OrderRecordDto>>> GetOrdersForSortingAsync()
        {
            try
            {
                var ordersResult = await GetAllAsync();
                if (ordersResult.IsFailed)
                    return Result.Fail<IEnumerable<OrderRecordDto>>(ordersResult.Errors);

                var allowedStatuses = new[] { "Partially Paid", "Fully Paid" };

                var filteredOrders = ordersResult.Value
                    .Where(o => allowedStatuses.Contains(o.OrderStatus))
                    .OrderByDescending(o => o.OrderDatetime)
                    .ToList();

                return Result.Ok<IEnumerable<OrderRecordDto>>(filteredOrders);
            }
            catch (Exception ex)
            {
                return Result.Fail<IEnumerable<OrderRecordDto>>(ex.Message);
            }
        }

        public async Task<Result<PaginatedList<OrderRecordDto>>> GetOrdersForReturnsAsync(
            string? searchString = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            int pageIndex = 1,
            int pageSize = 10)
        {
            try
            {
                var ordersResult = await GetAllAsync();
                if (ordersResult.IsFailed)
                    return Result.Fail<PaginatedList<OrderRecordDto>>(ordersResult.Errors);

                var completedOrders = ordersResult.Value
                    .Where(o => o.OrderStatus == "Completed"
                             && o.OrderPackages != null
                             && o.OrderPackages.Any())
                    .AsEnumerable();

                // Apply search filters
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    completedOrders = completedOrders.Where(o =>
                        o.CustomerName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        o.OrderRecordsId.ToString().Contains(searchString));
                }

                if (fromDate.HasValue)
                {
                    completedOrders = completedOrders.Where(o => DateOnly.FromDateTime(o.OrderDatetime) >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    completedOrders = completedOrders.Where(o => DateOnly.FromDateTime(o.OrderDatetime) <= toDate.Value);
                }

                var sortedOrders = completedOrders.OrderByDescending(o => o.OrderDatetime).ToList();

                var totalCount = sortedOrders.Count;
                var pagedOrders = sortedOrders
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var paginatedResult = new PaginatedList<OrderRecordDto>(pagedOrders, totalCount, pageIndex, pageSize);

                return Result.Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                return Result.Fail<PaginatedList<OrderRecordDto>>(ex.Message);
            }
        }

        public async Task<Result<PaginatedList<OrderRecordDto>>> GetStaffOrdersAsync(int pageIndex, int pageSize)
        {
            try
            {
                var ordersResult = await GetOrdersForSortingAsync(); // already returns Result<IEnumerable<OrderRecordDto>>
                if (ordersResult.IsFailed)
                    return Result.Fail<PaginatedList<OrderRecordDto>>(ordersResult.Errors);

                var sorted = ordersResult.Value.ToList(); // already newest first by the existing method
                var totalCount = sorted.Count;
                var page = sorted.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

                return Result.Ok(new PaginatedList<OrderRecordDto>(page, totalCount, pageIndex, pageSize));
            }
            catch (Exception ex)
            {
                return Result.Fail<PaginatedList<OrderRecordDto>>(ex.Message);
            }
        }

        public async Task<Result> CompleteOrderAsync(int orderId)
        {
            try
            {
                var orderRes = await GetByIdAsync(orderId);
                if (orderRes.IsFailed) return Result.Fail(orderRes.Errors);

                var order = orderRes.Value;

                // business rule: only Partially Paid or Fully Paid can be completed
                var allowed = new[] { "Partially Paid", "Fully Paid" };
                if (!allowed.Contains(order.OrderStatus))
                    return Result.Fail("Order cannot be completed. Only Partially Paid or Fully Paid orders can be marked as completed.");

                order.OrderStatus = "Completed";
                var updateRes = await UpdateAsync(order);
                if (updateRes.IsFailed) return Result.Fail(updateRes.Errors);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }


        private OrderRecordDto MapToDto(
            OrderRecord order,
            IEnumerable<ClothingOrderAndStockManagement.Domain.Entities.Customers.CustomerInfo> customers)
        {
            var customer = customers.FirstOrDefault(c => c.CustomerId == order.CustomerId);

            return new OrderRecordDto
            {
                OrderRecordsId = order.OrderRecordsId,
                CustomerId = order.CustomerId,
                CustomerName = customer?.CustomerName ?? "(Unknown)",
                OrderDatetime = order.OrderDatetime,
                OrderStatus = order.OrderStatus,
                OrderPackages = order.OrderPackages?.Select(p => new OrderPackageDto
                {
                    OrderPackagesId = p.OrderPackagesId,
                    PackagesId = p.PackagesId,
                    PackageName = p.Packages?.PackageName ?? "(Unknown Package)",
                    Quantity = p.Quantity,
                    PriceAtPurchase = p.PriceAtPurchase
                }).ToList() ?? new List<OrderPackageDto>(),
                PaymentRecords = order.PaymentRecords?.Select(pr => new PaymentRecordDto
                {
                    PaymentRecordsId = pr.PaymentRecordsId,
                    Amount = pr.Amount,
                    ProofUrl = pr.ProofUrl,
                    ProofUrl2 = pr.ProofUrl2,
                    PaymentStatus = pr.PaymentStatus,
                    PaymentDate = pr.PaymentDate
                }).ToList() ?? new List<PaymentRecordDto>()
            };
        }

        private static async Task<Result<string>> SavePaymentProofAsync(IFormFile file)
        {
            try
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                    return Result.Fail<string>("Only image files (JPG, JPEG, PNG, GIF) are allowed.");
                if (file.Length > 10 * 1024 * 1024)
                    return Result.Fail<string>("File size must be less than 10MB.");

                var baseDir = Path.Combine(Directory.GetCurrentDirectory(), "LocalStorage", "PaymentProofs");
                if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

                var name = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{ext}";
                var full = Path.Combine(baseDir, name);
                using (var s = new FileStream(full, FileMode.Create))
                    await file.CopyToAsync(s);

                var relativePath = Path.Combine("LocalStorage", "PaymentProofs", name).Replace("\\", "/");
                return Result.Ok(relativePath);
            }
            catch (Exception ex)
            {
                return Result.Fail<string>(ex.Message);
            }
        }
    }
}
