using ClothingOrderAndStockManagement.Domain.Interfaces.Repositories;
using ClothingOrderAndStockManagement.Domain.Entities.Orders;
using ClothingOrderAndStockManagement.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Application.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IApplicationDbContext _context;

        public OrderRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderRecord>> GetAllAsync()
        {
            return await _context.Set<OrderRecord>()
                .Include(o => o.OrderPackages)
                .Include(o => o.PaymentRecords)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<OrderRecord?> GetByIdAsync(int id)
        {
            return await _context.Set<OrderRecord>()
                .Include(o => o.OrderPackages)
                .Include(o => o.PaymentRecords)
                .FirstOrDefaultAsync(o => o.OrderRecordsId == id);
        }

        public async Task AddAsync(OrderRecord order)
        {
            await _context.Set<OrderRecord>().AddAsync(order);
        }

        public async Task UpdateAsync(OrderRecord order)
        {
            _context.Set<OrderRecord>().Update(order);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.Set<OrderRecord>()
                .Include(o => o.OrderPackages)
                .Include(o => o.PaymentRecords)
                .FirstOrDefaultAsync(o => o.OrderRecordsId == id);

            if (order != null)
            {
                _context.Set<OrderPackage>().RemoveRange(order.OrderPackages);
                _context.Set<PaymentRecord>().RemoveRange(order.PaymentRecords);
                _context.Set<OrderRecord>().Remove(order);
            }
        }

        public IQueryable<OrderRecord> Query()
        {
            return _context.Set<OrderRecord>().AsQueryable();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
