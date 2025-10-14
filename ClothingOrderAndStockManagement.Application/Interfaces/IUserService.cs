using ClothingOrderAndStockManagement.Application.Dtos.Customers;
using ClothingOrderAndStockManagement.Application.Dtos.Users;
using ClothingOrderAndStockManagement.Application.Helpers;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<PaginatedList<UserDto>> GetUsersAsync(string searchString, int pageIndex, int pageSize);
        Task<UserDto?> GetUserByIdAsync(string id);
        Task CreateUserAsync(CreateUserDto dto);
        Task UpdateUserAsync(EditUserDto dto);
        Task DeleteUserAsync(string id);
    }
}
