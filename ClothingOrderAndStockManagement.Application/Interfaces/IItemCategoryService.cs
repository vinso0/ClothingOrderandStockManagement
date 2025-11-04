using ClothingOrderAndStockManagement.Application.Dtos.Items;
using FluentResults;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface IItemCategoryService
    {
        Task<IEnumerable<ItemCategoryDto>> GetAllCategoriesAsync();
        Task<Result> AddCategoryAsync(CreateItemCategoryDto dto);
        Task<Result> UpdateCategoryAsync(UpdateItemCategoryDto dto);
        Task<Result> DeleteCategoryAsync(int id);
    }
}
