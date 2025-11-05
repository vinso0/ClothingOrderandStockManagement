
using ClothingOrderAndStockManagement.Application.Dtos.Items;
using ClothingOrderAndStockManagement.Application.Helpers;
using FluentResults;

namespace ClothingOrderAndStockManagement.Domain.Interfaces
{
    public interface IItemService
    {
        Task<PaginatedList<ItemDto>> GetItemsAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<ItemDto?> GetItemByIdAsync(int id);
        Task<IEnumerable<ItemCategoryDto>> GetItemCategoriesAsync();
        Task<Result> CreateItemAsync(CreateItemDto dto);
        Task<Result> UpdateItemAsync(UpdateItemDto dto);
        Task<Result> DeleteItemAsync(int id);
    }
}
