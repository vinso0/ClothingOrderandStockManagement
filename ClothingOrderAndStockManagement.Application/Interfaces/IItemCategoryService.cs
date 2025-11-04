using ClothingOrderAndStockManagement.Application.Dtos.Items;
using ClothingOrderAndStockManagement.Application.Helpers;
using FluentResults;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface IItemCategoryService
    {
        Task<IEnumerable<ItemCategoryDto>> GetAllCategoriesAsync();
        Task<PaginatedList<ItemCategoryDto>> GetCategoriesAsync(int pageIndex, int pageSize, string searchString = ""); // Add this
        Task<Result> AddCategoryAsync(CreateItemCategoryDto dto);
        Task<Result> UpdateCategoryAsync(UpdateItemCategoryDto dto);
        Task<Result> DeleteCategoryAsync(int id);
    }
}
