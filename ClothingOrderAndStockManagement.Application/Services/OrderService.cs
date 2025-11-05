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

        public OrderService(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IInventoryService inventoryService)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _inventoryService = inventoryService;
        }

        public async Task<IEnumerable<OrderRecordDto>> GetAllAsync()
        {
            var orders = await _orderRepository.Query()
                .Include(o => o.OrderPackages).ThenInclude(op => op.Packages)
                .Include(o => o.PaymentRecords)
                .ToListAsync();

            var customers = await _customerRepository.GetAllAsync();
            return orders.Select(o => MapToDto(o, customers));
        }

        public async Task<OrderRecordDto?> GetByIdAsync(int id)
        {
            var order = await _orderRepository.Query()
                .Include(o => o.OrderPackages).ThenInclude(p => p.Packages)
                .Include(o => o.PaymentRecords)
                .FirstOrDefaultAsync(o => o.OrderRecordsId == id);

            if (order == null) return null;

            var customers = await _customerRepository.GetAllAsync();
            return MapToDto(order, customers);
        }

        public async Task<int> CreateAsync(CreateOrderDto dto)
        {
            if (dto.OrderPackages == null || dto.OrderPackages.Count == 0)
                throw new InvalidOperationException("Add at least one package.");

            foreach (var p in dto.OrderPackages)
            {
                if (p.PackagesId <= 0 || p.Quantity <= 0)
                    throw new InvalidOperationException("Package and positive quantity are required.");
            }

            // Validate availability
            foreach (var p in dto.OrderPackages)
            {
                var ok = await _inventoryService.ValidatePackageAvailabilityAsync(p.PackagesId, p.Quantity);
                if (!ok) throw new InvalidOperationException("Insufficient package availability.");
            }

            // Reserve
            var reserved = new List<(int packageId, int qty)>();
            try
            {
                foreach (var p in dto.OrderPackages)
                {
                    var res = await _inventoryService.ReservePackageQuantityAsync(p.PackagesId, p.Quantity);
                    if (res.IsFailed) throw new InvalidOperationException(res.Errors[0].Message);
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
                return order.OrderRecordsId;
            }
            catch
            {
                // rollback reservations if failed
                foreach (var (pid, q) in reserved)
                    await _inventoryService.ReleasePackageQuantityAsync(pid, q);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(OrderRecordDto dto)
        {
            var existing = await _orderRepository.Query()
                .Include(o => o.OrderPackages)
                .Include(o => o.PaymentRecords)
                .FirstOrDefaultAsync(o => o.OrderRecordsId == dto.OrderRecordsId);

            if (existing == null) return false;

            // Example: if updating to Cancelled, release reservations
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
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _orderRepository.Query()
                .Include(o => o.OrderPackages)
                .FirstOrDefaultAsync(o => o.OrderRecordsId == id);

            if (existing != null)
            {
                // Release reservations on delete
                foreach (var p in existing.OrderPackages)
                    await _inventoryService.ReleasePackageQuantityAsync(p.PackagesId, p.Quantity);
            }

            await _orderRepository.DeleteAsync(id);
            await _orderRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddPaymentAsync(AddPaymentDto dto, IFormFile? proof1, IFormFile? proof2)
        {
            var order = await _orderRepository.Query()
                .Include(o => o.OrderPackages)
                .Include(o => o.PaymentRecords)
                .FirstOrDefaultAsync(o => o.OrderRecordsId == dto.OrderRecordsId);

            if (order == null) return false;

            string? url1 = null;
            if (proof1 != null) url1 = await SavePaymentProofAsync(proof1);

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
            return true;
        }

        public async Task<int> CreateWithPaymentAsync(CreateOrderDto dto, IFormFile? proof1, IFormFile? proof2)
        {
            // Same as CreateAsync; attach payment later if needed
            return await CreateAsync(dto);
        }

        public async Task<IEnumerable<OrderRecordDto>> GetOrdersForSortingAsync()
        {
            var orders = await _orderRepository.Query()
                .Include(o => o.OrderPackages).ThenInclude(op => op.Packages)
                .Include(o => o.PaymentRecords)
                .ToListAsync();

            var customers = await _customerRepository.GetAllAsync();

            // Filter for only Partially Paid and Fully Paid orders, sorted by date (newest first)
            var allowedStatuses = new[] { "Partially Paid", "Fully Paid" };

            return orders
                .Where(o => allowedStatuses.Contains(o.OrderStatus))
                .OrderByDescending(o => o.OrderDatetime)
                .Select(o => MapToDto(o, customers));
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
                var orders = await GetAllAsync();

                var completedOrders = orders
                    .Where(o => o.OrderStatus == "Completed" && o.OrderPackages.Any()) // Add this check
                    .AsEnumerable(); // Convert to IEnumerable for LINQ operations

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
                var error = new Error("Database").CausedBy(ex.Message);
                return Result.Fail<PaginatedList<OrderRecordDto>>(error);
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

        private static async Task<string> SavePaymentProofAsync(IFormFile file)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                throw new InvalidOperationException("Only image files (JPG, JPEG, PNG, GIF) are allowed.");
            if (file.Length > 10 * 1024 * 1024)
                throw new InvalidOperationException("File size must be less than 10MB.");

            var baseDir = Path.Combine(Directory.GetCurrentDirectory(), "LocalStorage", "PaymentProofs");
            if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

            var name = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{ext}";
            var full = Path.Combine(baseDir, name);
            using (var s = new FileStream(full, FileMode.Create))
                await file.CopyToAsync(s);

            return Path.Combine("LocalStorage", "PaymentProofs", name).Replace("\\", "/");
        }
    }
}
