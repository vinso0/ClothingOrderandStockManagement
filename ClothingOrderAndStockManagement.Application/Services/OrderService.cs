using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Application.Interfaces.Repositories;
using ClothingOrderAndStockManagement.Domain.Entities.Orders;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;

        // Allowed values
        private static readonly HashSet<string> AllowedOrderStatuses =
            new(new[] { "Awaiting Payment", "Partially Paid", "Fully Paid", "Completed", "Returned" });

        private static readonly HashSet<string> AllowedPaymentStatuses =
            new(new[] { "Down Payment", "Full Payment" });

        public OrderService(IOrderRepository orderRepository, ICustomerRepository customerRepository)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
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

        // Legacy style: accepts an OrderRecordDto already composed
        public async Task<int> CreateAsync(OrderRecordDto dto)
        {
            NormalizeStatuses(dto);
            var entity = MapToEntity(dto);

            // Derive order status if needed
            entity.OrderStatus = DeriveOrderStatus(
                entity.OrderPackages?.Sum(p => p.PriceAtPurchase * p.Quantity) ?? 0m,
                entity.PaymentRecords?.Sum(p => p.Amount) ?? 0m,
                entity.OrderStatus);

            await _orderRepository.AddAsync(entity);
            await _orderRepository.SaveChangesAsync();
            return entity.OrderRecordsId;
        }

        public async Task<bool> UpdateAsync(OrderRecordDto dto)
        {
            NormalizeStatuses(dto);

            var existing = await _orderRepository.Query()
                .Include(o => o.OrderPackages)
                .Include(o => o.PaymentRecords)
                .FirstOrDefaultAsync(o => o.OrderRecordsId == dto.OrderRecordsId);

            if (existing == null) return false;

            UpdateEntityFromDto(existing, dto);

            existing.OrderStatus = DeriveOrderStatus(
                existing.OrderPackages?.Sum(p => p.PriceAtPurchase * p.Quantity) ?? 0m,
                existing.PaymentRecords?.Sum(p => p.Amount) ?? 0m,
                existing.OrderStatus);

            await _orderRepository.UpdateAsync(existing);
            await _orderRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _orderRepository.DeleteAsync(id);
            await _orderRepository.SaveChangesAsync();
            return true;
        }

        // Primary MVC use case: accepts form DTO + files and persists aggregate.
        public async Task<int> CreateWithPaymentAsync(CreateOrderDto dto, IFormFile? proof1, IFormFile? proof2)
        {
            NormalizeStatuses(dto);

            // Build entity
            var order = new OrderRecord
            {
                CustomerId = dto.CustomerId,
                OrderDatetime = dto.OrderDatetime,
                UserId = dto.UserId ?? "System",
                OrderPackages = dto.OrderPackages?.Select(p => new OrderPackage
                {
                    PackagesId = p.PackagesId,
                    Quantity = p.Quantity,
                    PriceAtPurchase = p.PriceAtPurchase
                }).ToList() ?? new List<OrderPackage>(),
                PaymentRecords = new List<PaymentRecord>()
            };

            // Optional initial payment
            if (dto.InitialPayment != null && dto.InitialPayment.Amount > 0)
            {
                string? url1 = null, url2 = null;
                if (proof1 != null) url1 = await SavePaymentProofAsync(proof1);
                if (proof2 != null) url2 = await SavePaymentProofAsync(proof2);

                order.PaymentRecords.Add(new PaymentRecord
                {
                    CustomerId = dto.CustomerId,
                    Amount = dto.InitialPayment.Amount,
                    ProofUrl = url1,
                    ProofUrl2 = url2,
                    PaymentStatus = dto.InitialPayment.PaymentStatus,
                    PaymentDate = DateTime.Now
                });
            }

            // Derive status from totals
            var total = order.OrderPackages.Sum(p => p.PriceAtPurchase * p.Quantity);
            var paid = order.PaymentRecords.Sum(p => p.Amount);
            order.OrderStatus = DeriveOrderStatus(total, paid, dto.OrderStatus);

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();
            return order.OrderRecordsId;
        }

        // ————— helpers —————

        private static void NormalizeStatuses(CreateOrderDto dto)
        {
            // Normalize order status
            if (string.IsNullOrWhiteSpace(dto.OrderStatus) || !AllowedOrderStatuses.Contains(dto.OrderStatus))
            {
                dto.OrderStatus = "Awaiting Payment";
            }

            // Normalize payment status if present
            if (dto.InitialPayment != null)
            {
                if (string.IsNullOrWhiteSpace(dto.InitialPayment.PaymentStatus) ||
                    !AllowedPaymentStatuses.Contains(dto.InitialPayment.PaymentStatus))
                {
                    // Infer payment status from amounts
                    dto.InitialPayment.PaymentStatus =
                        dto.InitialPayment.Amount > 0 ? "Down Payment" : "Down Payment";
                }
            }
        }

        private static void NormalizeStatuses(OrderRecordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.OrderStatus) || !AllowedOrderStatuses.Contains(dto.OrderStatus))
                dto.OrderStatus = "Awaiting Payment";

            if (dto.PaymentRecords != null)
            {
                foreach (var pr in dto.PaymentRecords)
                {
                    if (string.IsNullOrWhiteSpace(pr.PaymentStatus) ||
                        !AllowedPaymentStatuses.Contains(pr.PaymentStatus))
                    {
                        pr.PaymentStatus = pr.Amount > 0 ? "Down Payment" : "Down Payment";
                    }
                }
            }
        }

        // Computes canonical order status given totals
        private static string DeriveOrderStatus(decimal total, decimal paid, string? requested)
        {
            // If caller set a valid terminal status Completed/Returned, keep it
            if (!string.IsNullOrWhiteSpace(requested) && AllowedOrderStatuses.Contains(requested) &&
                (requested == "Completed" || requested == "Returned"))
            {
                return requested;
            }

            if (total <= 0)
                return "Awaiting Payment"; // no packages yet

            if (paid <= 0)
                return "Awaiting Payment";

            if (paid < total)
                return "Partially Paid";

            return "Fully Paid";
        }

        private OrderRecordDto MapToDto(
            OrderRecord order,
            IEnumerable<ClothingOrderAndStockManagement.Domain.Entities.Customers.CustomerInfo> customers)
        {
            var customer = customers.FirstOrDefault(c => c.CustomerId == order.CustomerId);

            var dto = new OrderRecordDto
            {
                OrderRecordsId = order.OrderRecordsId,
                CustomerId = order.CustomerId,
                CustomerName = customer?.CustomerName ?? "(Unknown)",
                OrderDatetime = order.OrderDatetime,
                OrderStatus = order.OrderStatus,
                UserId = order.UserId,
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

            // Do not assign TotalAmount if it is read-only; let the DTO compute it.

            return dto;
        }

        private static OrderRecord MapToEntity(OrderRecordDto dto)
        {
            return new OrderRecord
            {
                OrderRecordsId = dto.OrderRecordsId,
                CustomerId = dto.CustomerId,
                OrderDatetime = dto.OrderDatetime,
                OrderStatus = dto.OrderStatus,
                UserId = dto.UserId,
                OrderPackages = dto.OrderPackages?.Select(p => new OrderPackage
                {
                    PackagesId = p.PackagesId,
                    Quantity = p.Quantity,
                    PriceAtPurchase = p.PriceAtPurchase
                }).ToList() ?? new List<OrderPackage>(),
                PaymentRecords = dto.PaymentRecords?.Select(pr => new PaymentRecord
                {
                    CustomerId = dto.CustomerId,
                    Amount = pr.Amount,
                    ProofUrl = pr.ProofUrl,
                    ProofUrl2 = pr.ProofUrl2,
                    PaymentStatus = pr.PaymentStatus,
                    PaymentDate = pr.PaymentDate
                }).ToList() ?? new List<PaymentRecord>()
            };
        }

        private static void UpdateEntityFromDto(OrderRecord entity, OrderRecordDto dto)
        {
            entity.OrderStatus = dto.OrderStatus;
            entity.UserId = dto.UserId;

            entity.OrderPackages.Clear();
            if (dto.OrderPackages != null)
            {
                foreach (var p in dto.OrderPackages)
                {
                    entity.OrderPackages.Add(new OrderPackage
                    {
                        OrderRecordsId = entity.OrderRecordsId,
                        PackagesId = p.PackagesId,
                        Quantity = p.Quantity,
                        PriceAtPurchase = p.PriceAtPurchase
                    });
                }
            }

            entity.PaymentRecords.Clear();
            if (dto.PaymentRecords != null)
            {
                foreach (var pr in dto.PaymentRecords)
                {
                    entity.PaymentRecords.Add(new PaymentRecord
                    {
                        OrderRecordsId = entity.OrderRecordsId,
                        CustomerId = dto.CustomerId,
                        Amount = pr.Amount,
                        ProofUrl = pr.ProofUrl,
                        ProofUrl2 = pr.ProofUrl2,
                        PaymentStatus = pr.PaymentStatus,
                        PaymentDate = pr.PaymentDate
                    });
                }
            }
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