using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Interfaces.Repositories;
using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Orders;
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

        // 🔹 Get all orders (fixed + optimized)
        public async Task<IEnumerable<OrderRecordDto>> GetAllAsync()
        {
            // Fetch all orders with navigation properties
            var orders = await _orderRepository.Query()
                .Include(o => o.OrderPackages)
                    .ThenInclude(op => op.Packages)
                .Include(o => o.PaymentRecords)
                .ToListAsync();

            // Fetch all customers (for mapping)
            var customers = await _customerRepository.GetAllAsync();

            // Map to DTOs
            var orderDtos = orders.Select(o =>
            {
                var customer = customers.FirstOrDefault(c => c.CustomerId == o.CustomerId);

                return new OrderRecordDto
                {
                    OrderRecordsId = o.OrderRecordsId,
                    CustomerId = o.CustomerId,
                    CustomerName = customer?.CustomerName ?? "(Unknown)",
                    OrderDatetime = o.OrderDatetime,
                    OrderStatus = o.OrderStatus,
                    UserId = o.UserId,
                    OrderPackages = o.OrderPackages.Select(p => new OrderPackageDto
                    {
                        OrderPackagesId = p.OrderPackagesId,
                        PackagesId = p.PackagesId,
                        PackageName = p.Packages.PackageName, // include this if Package has a name
                        Quantity = p.Quantity,
                        PriceAtPurchase = p.PriceAtPurchase
                    }).ToList(),
                    PaymentRecords = o.PaymentRecords.Select(pr => new PaymentRecordDto
                    {
                        PaymentRecordsId = pr.PaymentRecordsId,
                        Amount = pr.Amount,
                        ProofUrl = pr.ProofUrl,
                        PaymentStatus = pr.PaymentStatus
                    }).ToList()
                };
            });

            return orderDtos;
        }

        // 🔹 Get order by ID
        public async Task<OrderRecordDto?> GetByIdAsync(int id)
        {
            var order = await _orderRepository.Query()
                .Include(o => o.OrderPackages)
                    .ThenInclude(p => p.Packages)
                .Include(o => o.PaymentRecords)
                .FirstOrDefaultAsync(o => o.OrderRecordsId == id);

            if (order == null) return null;

            var customer = (await _customerRepository.GetAllAsync())
                .FirstOrDefault(c => c.CustomerId == order.CustomerId);

            return new OrderRecordDto
            {
                OrderRecordsId = order.OrderRecordsId,
                CustomerId = order.CustomerId,
                CustomerName = customer?.CustomerName ?? "(Unknown)",
                OrderDatetime = order.OrderDatetime,
                OrderStatus = order.OrderStatus,
                UserId = order.UserId,
                OrderPackages = order.OrderPackages.Select(p => new OrderPackageDto
                {
                    OrderPackagesId = p.OrderPackagesId,
                    PackagesId = p.PackagesId,
                    PackageName = p.Packages.PackageName,
                    Quantity = p.Quantity,
                    PriceAtPurchase = p.PriceAtPurchase
                }).ToList(),
                PaymentRecords = order.PaymentRecords.Select(pr => new PaymentRecordDto
                {
                    PaymentRecordsId = pr.PaymentRecordsId,
                    Amount = pr.Amount,
                    ProofUrl = pr.ProofUrl,
                    PaymentStatus = pr.PaymentStatus
                }).ToList()
            };
        }

        // 🔹 Create new order
        public async Task<int> CreateAsync(OrderRecordDto orderDto)
        {
            var order = new OrderRecord
            {
                CustomerId = orderDto.CustomerId,
                OrderDatetime = DateTime.UtcNow,
                OrderStatus = orderDto.OrderStatus,
                UserId = orderDto.UserId,
                OrderPackages = orderDto.OrderPackages.Select(p => new OrderPackage
                {
                    PackagesId = p.PackagesId,
                    Quantity = p.Quantity,
                    PriceAtPurchase = p.PriceAtPurchase
                }).ToList(),
                PaymentRecords = orderDto.PaymentRecords.Select(pr => new PaymentRecord
                {
                    CustomerId = orderDto.CustomerId,
                    Amount = pr.Amount,
                    ProofUrl = pr.ProofUrl,
                    PaymentStatus = pr.PaymentStatus
                }).ToList()
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            return order.OrderRecordsId;
        }

        // 🔹 Update order
        public async Task<bool> UpdateAsync(OrderRecordDto orderDto)
        {
            var existingOrder = await _orderRepository.GetByIdAsync(orderDto.OrderRecordsId);
            if (existingOrder == null) return false;

            existingOrder.OrderStatus = orderDto.OrderStatus;
            existingOrder.UserId = orderDto.UserId;

            // Replace child collections
            existingOrder.OrderPackages.Clear();
            existingOrder.OrderPackages = orderDto.OrderPackages.Select(p => new OrderPackage
            {
                OrderRecordsId = existingOrder.OrderRecordsId,
                PackagesId = p.PackagesId,
                Quantity = p.Quantity,
                PriceAtPurchase = p.PriceAtPurchase
            }).ToList();

            existingOrder.PaymentRecords.Clear();
            existingOrder.PaymentRecords = orderDto.PaymentRecords.Select(pr => new PaymentRecord
            {
                OrderRecordsId = existingOrder.OrderRecordsId,
                CustomerId = orderDto.CustomerId,
                Amount = pr.Amount,
                ProofUrl = pr.ProofUrl,
                PaymentStatus = pr.PaymentStatus
            }).ToList();

            await _orderRepository.UpdateAsync(existingOrder);
            await _orderRepository.SaveChangesAsync();

            return true;
        }

        // 🔹 Delete order
        public async Task<bool> DeleteAsync(int id)
        {
            await _orderRepository.DeleteAsync(id);
            await _orderRepository.SaveChangesAsync();
            return true;
        }
    }
}
