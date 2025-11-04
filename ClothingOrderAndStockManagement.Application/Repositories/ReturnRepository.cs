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
            // Only orders with Status == "Completed"
            // Include minimal navigations needed for projection in service
            return _context.Set<OrderRecord>()
                .AsNoTracking()
                .Where(o => o.Status == "Completed")
                .Include(o => o.CustomerInfo)
                .Include(o => o.OrderPackages);
        }

        public IQueryable<ReturnLog> GetReturnsQuery()
        {
            // Return history query with relationships used by service projection
            return _context.Set<ReturnLog>()
                .AsNoTracking()
                .Include(r => r.CustomerInfo)
                .Include(r => r.OrderRecords)
                .Include(r => r.OrderPackage);
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

            order.Status = "Returned";
            _context.Set<OrderRecord>().Update(order);
            return true;
        }

        public async Task<bool> RestockItemsAsync(int orderPackagesId)
        {
            // Increase package stock by the quantity sold in this order package
            var orderPackage = await _context.Set<OrderPackage>()
                .Include(op => op.Package) // ensure Package is loaded to mutate stock
                .FirstOrDefaultAsync(op => op.OrderPackagesId == orderPackagesId);

            if (orderPackage == null || orderPackage.Package == null)
                return false;

            // Add back the quantity to package stock
            orderPackage.Package.StockQuantity += orderPackage.Quantity;

            // If you also decrement per-item stock elsewhere, adjust here as needed
            _context.Set<Package>().Update(orderPackage.Package);
            return true;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
