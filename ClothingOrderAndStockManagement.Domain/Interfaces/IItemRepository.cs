using ClothingOrderAndStockManagement.Domain.Entities.Products;

namespace ClothingOrderAndStockManagement.Domain.Interfaces
{
    public interface IItemRepository
    {
        Task<IEnumerable<Item>> GetAllAsync();
        Task<Item?> GetByIdAsync(int id);
        Task AddAsync(Item item);
        Task UpdateAsync(Item item);
        Task DeleteAsync(int id);
        IQueryable<Item> Query();
        Task<IEnumerable<ItemCategory>> GetItemCategoriesAsync();
        Task<bool> ItemExistsAsync(int itemId);
        Task SaveChangesAsync();
    }
}
