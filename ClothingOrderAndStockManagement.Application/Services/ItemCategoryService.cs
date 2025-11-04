using ClothingOrderAndStockManagement.Application.Dtos.Items;
using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Products;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using FluentResults;

public class ItemCategoryService : IItemCategoryService
{
    private readonly IItemCategoryRepository _repo;
    private readonly IItemRepository _itemRepo;

    public ItemCategoryService(IItemCategoryRepository repo, IItemRepository itemRepo)
    {
        _repo = repo;
        _itemRepo = itemRepo;
    }

    public async Task<IEnumerable<ItemCategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _repo.Query()
            .Select(c => new ItemCategoryDto
            {
                ItemCategoryId = c.ItemCategoryId,
                ItemCategoryType = c.ItemCategoryType,
                ItemsCount = c.Items.Count() // Count related items
            })
            .ToListAsync();

        return categories;
    }

    public async Task<Result> AddCategoryAsync(CreateItemCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ItemCategoryType))
            return Result.Fail("Category name is required.");

        await _repo.AddAsync(new ItemCategory
        {
            ItemCategoryType = dto.ItemCategoryType.Trim()
        });
        await _repo.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> UpdateCategoryAsync(UpdateItemCategoryDto dto)
    {
        var existing = await _repo.GetByIdAsync(dto.ItemCategoryId);
        if (existing == null)
            return Result.Fail("Category not found.");

        existing.ItemCategoryType = dto.ItemCategoryType.Trim();
        await _repo.UpdateAsync(existing);
        await _repo.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteCategoryAsync(int id)
    {
        // Check if any items use this category
        var hasItems = await _itemRepo.Query().AnyAsync(i => i.ItemCategoryId == id);
        if (hasItems)
            return Result.Fail("Cannot delete category that has items assigned to it.");

        await _repo.DeleteAsync(id);
        await _repo.SaveChangesAsync();
        return Result.Ok();
    }
}
