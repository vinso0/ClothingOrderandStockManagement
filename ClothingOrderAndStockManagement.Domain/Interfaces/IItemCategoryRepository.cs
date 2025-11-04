using ClothingOrderAndStockManagement.Domain.Entities.Products;

namespace ClothingOrderAndStockManagement.Domain.Interfaces
{
    public interface IItemCategoryRepository
    {
        IQueryable<ItemCategory> Query();
        Task<ItemCategory?> GetByIdAsync(int id);
        Task AddAsync(ItemCategory category);
        Task UpdateAsync(ItemCategory category);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
    }
}
