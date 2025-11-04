using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Orders;
using ClothingOrderAndStockManagement.Domain.Entities.Products;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Infrastructure.Data
{
    public class ReturnRepository : IReturnRepository
    {
        private readonly IApplicationDbContext _context;

        public ReturnRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<OrderRecord> GetCompletedOrdersQuery()
        {
            return _context.Set<OrderRecord>()
                .AsNoTracking()
                .Where(o => o.OrderStatus == "Completed")
                .Include(o => o.OrderPackages)
                    .ThenInclude(op => op.Packages);
        }

        public IQueryable<ReturnLog> GetReturnsQuery()
        {
            return _context.Set<ReturnLog>()
                .AsNoTracking()
                .Include(r => r.CustomerInfo)
                .Include(r => r.OrderRecords)
                .Include(r => r.OrderPackage)
                    .ThenInclude(op => op.Packages);
        }

        public async Task AddReturnLogAsync(ReturnLog returnLog)
        {
            await _context.Set<ReturnLog>().AddAsync(returnLog);
        }

        public async Task<bool> UpdateOrderStatusToReturnedAsync(int orderRecordsId)
        {
            var order = await _context.Set<OrderRecord>()
                .FirstOrDefaultAsync(o => o.OrderRecordsId == orderRecordsId);

            if (order == null)
                return false;

            order.OrderStatus = "Returned";
            _context.Set<OrderRecord>().Update(order);
            return true;
        }

        public async Task<bool> RestockItemsAsync(int orderPackagesId)
        {
            var orderPackage = await _context.Set<OrderPackage>()
                .Include(op => op.Packages)
                .FirstOrDefaultAsync(op => op.OrderPackagesId == orderPackagesId);

            if (orderPackage == null || orderPackage.Packages == null)
                return false;

            // Use QuantityAvailable (actual property on Package)
            orderPackage.Packages.QuantityAvailable += orderPackage.Quantity;

            _context.Set<Package>().Update(orderPackage.Packages);
            return true;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
