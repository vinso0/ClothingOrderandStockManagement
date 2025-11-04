using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Products;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Infrastructure.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly IApplicationDbContext _context;

        public ItemRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            return await _context.Set<Item>()
                .Include(i => i.ItemCategory)
                .OrderBy(i => i.ItemId)
                .ToListAsync();
        }

        public async Task<Item?> GetByIdAsync(int id)
        {
            return await _context.Set<Item>()
                .Include(i => i.ItemCategory)
                .FirstOrDefaultAsync(i => i.ItemId == id);
        }

        public async Task AddAsync(Item item)
        {
            _context.Set<Item>().Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Item item)
        {
            _context.Set<Item>().Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.Set<Item>().FindAsync(id);
            if (item != null)
            {
                _context.Set<Item>().Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public IQueryable<Item> Query()
        {
            return _context.Set<Item>().Include(i => i.ItemCategory);
        }

        public async Task<IEnumerable<ItemCategory>> GetItemCategoriesAsync()
        {
            return await _context.Set<ItemCategory>().OrderBy(ic => ic.ItemCategoryType).ToListAsync();
        }

        public async Task<bool> ItemExistsAsync(int itemId)
        {
            return await _context.Set<Item>().AnyAsync(i => i.ItemId == itemId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
