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

        #region Public Service Methods

        public async Task<IEnumerable<OrderRecordDto>> GetAllAsync()
        {
            var orders = await _orderRepository.Query()
                .Include(o => o.OrderPackages)
                    .ThenInclude(op => op.Packages)
                .Include(o => o.PaymentRecords)
                .ToListAsync();

            var customers = await _customerRepository.GetAllAsync();
            return orders.Select(o => MapToDto(o, customers));
        }

        public async Task<OrderRecordDto?> GetByIdAsync(int id)
        {
            var order = await _orderRepository.Query()
                .Include(o => o.OrderPackages)
                    .ThenInclude(p => p.Packages)
                .Include(o => o.PaymentRecords)
                .FirstOrDefaultAsync(o => o.OrderRecordsId == id);

            if (order == null) return null;

            var customers = await _customerRepository.GetAllAsync();
            return MapToDto(order, customers);
        }

        public async Task<int> CreateAsync(OrderRecordDto orderDto)
        {
            var order = MapToEntity(orderDto);
            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();
            return order.OrderRecordsId;
        }

        public async Task<bool> UpdateAsync(OrderRecordDto orderDto)
        {
            var existingOrder = await _orderRepository.Query()
                .Include(o => o.OrderPackages)
                .Include(o => o.PaymentRecords)
                .FirstOrDefaultAsync(o => o.OrderRecordsId == orderDto.OrderRecordsId);

            if (existingOrder == null) return false;

            UpdateEntityFromDto(existingOrder, orderDto);
            await _orderRepository.UpdateAsync(existingOrder);
            await _orderRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _orderRepository.DeleteAsync(id);
            await _orderRepository.SaveChangesAsync();
            return true;
        }

        public async Task<int> CreateWithPaymentAsync(CreateOrderDto dto, IFormFile? proof1, IFormFile? proof2)
        {
            // Handle file uploads if payment provided
            string? proofUrl1 = null, proofUrl2 = null;

            if (dto.InitialPayment != null && dto.InitialPayment.Amount > 0)
            {
                if (proof1 != null)
                    proofUrl1 = await SavePaymentProofAsync(proof1);

                if (proof2 != null)
                    proofUrl2 = await SavePaymentProofAsync(proof2);
            }

            // Create the order entity directly (bypass DTO mapping overhead)
            var order = CreateOrderFromDto(dto, proofUrl1, proofUrl2);

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            return order.OrderRecordsId;
        }





        #endregion

        #region Private Helper Methods

        private OrderRecordDto MapToDto(OrderRecord order, IEnumerable<ClothingOrderAndStockManagement.Domain.Entities.Customers.CustomerInfo> customers)
        {
            var customer = customers.FirstOrDefault(c => c.CustomerId == order.CustomerId);

            var orderDto = new OrderRecordDto
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

            // TotalAmount is calculated automatically by the DTO's getter
            // No need to assign it manually since it's read-only

            return orderDto;
        }


        private OrderRecord MapToEntity(OrderRecordDto dto)
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

        private OrderRecord CreateOrderFromDto(CreateOrderDto dto, string? proofUrl1, string? proofUrl2)
        {
            var order = new OrderRecord
            {
                CustomerId = dto.CustomerId,
                OrderDatetime = dto.OrderDatetime,
                OrderStatus = dto.OrderStatus,
                UserId = dto.UserId ?? "System",
                OrderPackages = dto.OrderPackages?.Select(p => new OrderPackage
                {
                    PackagesId = p.PackagesId,
                    Quantity = p.Quantity,
                    PriceAtPurchase = p.PriceAtPurchase
                }).ToList() ?? new List<OrderPackage>(),
                PaymentRecords = new List<PaymentRecord>()
            };

            // Add initial payment if provided
            if (dto.InitialPayment != null && dto.InitialPayment.Amount > 0)
            {
                var paymentRecord = new PaymentRecord
                {
                    CustomerId = dto.CustomerId,
                    Amount = dto.InitialPayment.Amount,
                    ProofUrl = proofUrl1,
                    ProofUrl2 = proofUrl2,
                    PaymentStatus = dto.InitialPayment.PaymentStatus,
                    PaymentDate = DateTime.Now
                };

                order.PaymentRecords.Add(paymentRecord);
            }

            return order;
        }

        private void UpdateEntityFromDto(OrderRecord entity, OrderRecordDto dto)
        {
            entity.OrderStatus = dto.OrderStatus;
            entity.UserId = dto.UserId;

            // Update OrderPackages (clear and rebuild to handle additions/removals)
            entity.OrderPackages.Clear();
            if (dto.OrderPackages != null)
            {
                foreach (var packageDto in dto.OrderPackages)
                {
                    entity.OrderPackages.Add(new OrderPackage
                    {
                        OrderRecordsId = entity.OrderRecordsId,
                        PackagesId = packageDto.PackagesId,
                        Quantity = packageDto.Quantity,
                        PriceAtPurchase = packageDto.PriceAtPurchase
                    });
                }
            }

            // Update PaymentRecords (clear and rebuild)
            entity.PaymentRecords.Clear();
            if (dto.PaymentRecords != null)
            {
                foreach (var paymentDto in dto.PaymentRecords)
                {
                    entity.PaymentRecords.Add(new PaymentRecord
                    {
                        OrderRecordsId = entity.OrderRecordsId,
                        CustomerId = dto.CustomerId,
                        Amount = paymentDto.Amount,
                        ProofUrl = paymentDto.ProofUrl,
                        ProofUrl2 = paymentDto.ProofUrl2,
                        PaymentStatus = paymentDto.PaymentStatus,
                        PaymentDate = paymentDto.PaymentDate
                    });
                }
            }
        }

        private async Task<string> SavePaymentProofAsync(IFormFile file)
        {
            // Validation
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                throw new InvalidOperationException("Only image files (JPG, JPEG, PNG, GIF) are allowed.");

            if (file.Length > 10 * 1024 * 1024) // 10MB
                throw new InvalidOperationException("File size must be less than 10MB.");

            // Create directory
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "LocalStorage", "PaymentProofs");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var uniqueFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}[..8]{fileExtension}";
            var fullPath = Path.Combine(uploadsPath, uniqueFileName);

            // Save file
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(fileStream);

            // Return relative path for database storage
            return Path.Combine("LocalStorage", "PaymentProofs", uniqueFileName).Replace("\\", "/");
        }

        #endregion
    }
}
