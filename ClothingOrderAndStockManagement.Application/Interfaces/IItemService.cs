
using ClothingOrderAndStockManagement.Application.Dtos.Items;
using ClothingOrderAndStockManagement.Application.Helpers;

namespace ClothingOrderAndStockManagement.Domain.Interfaces
{
    public interface IItemService
    {
        Task<PaginatedList<ItemDto>> GetItemsAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<ItemDto?> GetItemByIdAsync(int itemId);
        Task<ItemDto> CreateItemAsync(CreateItemDto createItemDto);
        Task<ItemDto> UpdateItemAsync(UpdateItemDto updateItemDto);
        Task<bool> DeleteItemAsync(int itemId);
        Task<IEnumerable<ItemCategoryDto>> GetItemCategoriesAsync();
    }
}
