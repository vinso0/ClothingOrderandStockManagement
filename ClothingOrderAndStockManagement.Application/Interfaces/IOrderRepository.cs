using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Domain.Entities.Orders;

namespace ClothingOrderAndStockManagement.Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderRecord>> GetAllAsync();
        Task<OrderRecord?> GetByIdAsync(int id);
        Task AddAsync(OrderRecord order);
        Task UpdateAsync(OrderRecord order);
        Task DeleteAsync(int id);
        IQueryable<OrderRecord> Query();
        Task SaveChangesAsync();
    }
}
