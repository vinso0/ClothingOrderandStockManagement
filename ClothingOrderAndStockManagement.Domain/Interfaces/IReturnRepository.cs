using ClothingOrderAndStockManagement.Domain.Entities.Orders;

namespace ClothingOrderAndStockManagement.Domain.Interfaces
{
    public interface IReturnRepository
    {
        // Queries returned as IQueryable so Application layer handles filtering/paging/projection
        IQueryable<OrderRecord> GetCompletedOrdersQuery();
        IQueryable<ReturnLog> GetReturnsQuery();

        // Commands
        Task AddReturnLogAsync(ReturnLog returnLog);
        Task<bool> UpdateOrderStatusToReturnedAsync(int orderRecordsId);
        Task<bool> RestockItemsAsync(int orderPackagesId);

        // Persistence
        Task SaveChangesAsync();
    }
}
