using ClothingOrderAndStockManagement.Application.Dtos.Items;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Products;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Application.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IInventoryService _inventoryService;

        public ItemService(
            IItemRepository itemRepository,
            IPackageRepository packageRepository,
            IInventoryService inventoryService)
        {
            _itemRepository = itemRepository;
            _packageRepository = packageRepository;
            _inventoryService = inventoryService;
        }

        public async Task<PaginatedList<ItemDto>> GetItemsAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _itemRepository.Query();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(i =>
                    i.ItemCategory.ItemCategoryType.Contains(searchTerm) ||
                    (i.Size != null && i.Size.Contains(searchTerm)) ||
                    (i.Color != null && i.Color.Contains(searchTerm)));
            }

            query = query.OrderBy(i => i.ItemId);

            var paginatedItems = await PaginatedList<Item>.CreateAsync(query, pageNumber, pageSize);

            var itemDtos = paginatedItems.Select(item => new ItemDto
            {
                ItemId = item.ItemId,
                ItemCategoryId = item.ItemCategoryId,
                Size = item.Size,
                Color = item.Color,
                Quantity = item.Quantity,
                ItemCategoryType = item.ItemCategory.ItemCategoryType
            }).ToList();

            return new PaginatedList<ItemDto>(itemDtos, paginatedItems.TotalCount, paginatedItems.PageIndex, pageSize);
        }

        public async Task<ItemDto?> GetItemByIdAsync(int itemId)
        {
            var item = await _itemRepository.GetByIdAsync(itemId);

            if (item == null) return null;

            return new ItemDto
            {
                ItemId = item.ItemId,
                ItemCategoryId = item.ItemCategoryId,
                Size = item.Size,
                Color = item.Color,
                Quantity = item.Quantity,
                ItemCategoryType = item.ItemCategory.ItemCategoryType
            };
        }

        public async Task<ItemDto> CreateItemAsync(CreateItemDto createItemDto)
        {
            var item = new Item
            {
                ItemCategoryId = createItemDto.ItemCategoryId,
                Size = createItemDto.Size,
                Color = createItemDto.Color,
                Quantity = createItemDto.Quantity
            };

            await _itemRepository.AddAsync(item);
            var createdItem = await _itemRepository.GetByIdAsync(item.ItemId);

            await UpdateAffectedPackageQuantitiesAsync(item.ItemId);

            return new ItemDto
            {
                ItemId = createdItem!.ItemId,
                ItemCategoryId = createdItem.ItemCategoryId,
                Size = createdItem.Size,
                Color = createdItem.Color,
                Quantity = createdItem.Quantity,
                ItemCategoryType = createdItem.ItemCategory.ItemCategoryType
            };
        }

        public async Task<ItemDto> UpdateItemAsync(UpdateItemDto updateItemDto)
        {
            var existingItem = await _itemRepository.GetByIdAsync(updateItemDto.ItemId);
            if (existingItem == null)
            {
                throw new ArgumentException("Item not found.");
            }

            existingItem.ItemCategoryId = updateItemDto.ItemCategoryId;
            existingItem.Size = updateItemDto.Size;
            existingItem.Color = updateItemDto.Color;
            existingItem.Quantity = updateItemDto.Quantity;

            await _itemRepository.UpdateAsync(existingItem);

            await UpdateAffectedPackageQuantitiesAsync(updateItemDto.ItemId);

            var updatedItem = await _itemRepository.GetByIdAsync(existingItem.ItemId);

            return new ItemDto
            {
                ItemId = updatedItem!.ItemId,
                ItemCategoryId = updatedItem.ItemCategoryId,
                Size = updatedItem.Size,
                Color = updatedItem.Color,
                Quantity = updatedItem.Quantity,
                ItemCategoryType = updatedItem.ItemCategory.ItemCategoryType
            };
        }

        public async Task<bool> DeleteItemAsync(int itemId)
        {
            var exists = await _itemRepository.ItemExistsAsync(itemId);
            if (!exists) return false;

            await UpdateAffectedPackageQuantitiesAsync(itemId);

            await _itemRepository.DeleteAsync(itemId);
            return true;
        }

        public async Task<IEnumerable<ItemCategoryDto>> GetItemCategoriesAsync()
        {
            var categories = await _itemRepository.GetItemCategoriesAsync();

            return categories.Select(c => new ItemCategoryDto
            {
                ItemCategoryId = c.ItemCategoryId,
                ItemCategoryType = c.ItemCategoryType
            });
        }

        private async Task UpdateAffectedPackageQuantitiesAsync(int itemId)
        {
            try
            {
                var affectedPackageIds = await _packageRepository.Query()
                    .Where(p => p.PackageItems.Any(pi => pi.ItemId == itemId))
                    .Select(p => p.PackagesId)
                    .ToListAsync();

                foreach (var packageId in affectedPackageIds)
                {
                    await _inventoryService.UpdatePackageQuantityAsync(packageId);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't break the main operation
                Console.WriteLine($"Error updating package quantities: {ex.Message}");
            }
        }
    }
}
