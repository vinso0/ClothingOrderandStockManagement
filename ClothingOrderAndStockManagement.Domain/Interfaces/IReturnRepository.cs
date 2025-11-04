using ClothingOrderAndStockManagement.Domain.Entities.Orders;

namespace ClothingOrderAndStockManagement.Domain.Interfaces
{
    public interface IReturnRepository
    {
        IQueryable<OrderRecord> GetCompletedOrdersQuery();
        IQueryable<ReturnLog> GetReturnsQuery();
        Task AddReturnLogAsync(ReturnLog returnLog);
        Task<bool> UpdateOrderStatusToReturnedAsync(int orderRecordsId);
        Task<bool> RestockItemsAsync(int orderPackagesId);
        Task SaveChangesAsync();
    }
}
