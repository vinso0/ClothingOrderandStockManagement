using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Products;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Infrastructure.Repositories
{
    public class ItemCategoryRepository : IItemCategoryRepository
    {
        private readonly IApplicationDbContext _context;

        public ItemCategoryRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<ItemCategory> Query()
        {
            return _context.Set<ItemCategory>().AsQueryable();
        }

        public async Task<ItemCategory?> GetByIdAsync(int id)
        {
            return await _context.Set<ItemCategory>().FindAsync(id);
        }

        public async Task AddAsync(ItemCategory category)
        {
            await _context.Set<ItemCategory>().AddAsync(category);
        }

        public async Task UpdateAsync(ItemCategory category)
        {
            _context.Set<ItemCategory>().Update(category);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var category = await GetByIdAsync(id);
            if (category != null)
            {
                _context.Set<ItemCategory>().Remove(category);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
