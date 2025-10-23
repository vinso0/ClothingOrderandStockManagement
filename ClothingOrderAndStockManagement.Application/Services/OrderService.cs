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

        public async Task<int> CreateAsync(CreateOrderDto dto)
        {
            if (dto.OrderPackages == null || dto.OrderPackages.Count == 0)
                throw new InvalidOperationException("Add at least one package.");

            foreach (var p in dto.OrderPackages)
            {
                if (p.PackagesId <= 0 || p.Quantity <= 0)
                    throw new InvalidOperationException("Package and positive quantity are required.");
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

        public async Task<bool> UpdateAsync(OrderRecordDto dto)
        {
            var existing = await _orderRepository.Query()
                .Include(o => o.OrderPackages)
                .Include(o => o.PaymentRecords)
                .FirstOrDefaultAsync(o => o.OrderRecordsId == dto.OrderRecordsId);

            if (existing == null) return false;

            existing.OrderStatus = dto.OrderStatus;

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




        // Add payment to existing order
        public async Task<bool> AddPaymentAsync(AddPaymentDto dto, IFormFile? proof1, IFormFile? proof2)
        {
            var order = await _orderRepository.Query()
                .Include(o => o.OrderPackages)
                .Include(o => o.PaymentRecords)
                .FirstOrDefaultAsync(o => o.OrderRecordsId == dto.OrderRecordsId);

            if (order == null) return false;

            // Save payment proof
            string? url1 = null;
            if (proof1 != null) url1 = await SavePaymentProofAsync(proof1);

            // Calculate totals
            var totalAmount = order.OrderPackages.Sum(p => p.PriceAtPurchase * p.Quantity);
            var currentTotalPaid = order.PaymentRecords.Sum(p => p.Amount);
            var newTotalPaid = currentTotalPaid + dto.Amount;

            // Determine payment status based on logic
            string paymentStatus = dto.PaymentStatus;

            // If this payment completes the order, force it to be "Full Payment"
            if (newTotalPaid >= totalAmount)
            {
                paymentStatus = "Full Payment";
            }

            // Add payment record
            var payment = new PaymentRecord
            {
                OrderRecordsId = dto.OrderRecordsId,
                CustomerId = order.CustomerId,
                Amount = dto.Amount,
                ProofUrl = url1,
                PaymentStatus = paymentStatus,
                PaymentDate = DateTime.Now
            };

            order.PaymentRecords.Add(payment);

            // Update order status based on total payments
            if (newTotalPaid >= totalAmount)
            {
                order.OrderStatus = "Fully Paid";
            }
            else if (newTotalPaid > 0)
            {
                order.OrderStatus = "Partially Paid";
            }
            else
            {
                order.OrderStatus = "Awaiting Payment";
            }

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
            return true;
        }

        public async Task<int> CreateWithPaymentAsync(CreateOrderDto dto, IFormFile? proof1, IFormFile? proof2)
        {
            return await CreateAsync(dto);
        }

        // Helper methods
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