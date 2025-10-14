using ClothingOrderAndStockManagement.Application.Dtos.Users;
using ClothingOrderAndStockManagement.Application.Helpers;
using FluentResults;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<PaginatedList<UserDto>>> GetUsersAsync(string searchString, int pageIndex, int pageSize);
        Task<Result<UserDto>> GetUserByIdAsync(string id);
        Task<Result> CreateUserAsync(CreateUserDto dto);
        Task<Result> UpdateUserAsync(EditUserDto dto);
        Task<Result> DeleteUserAsync(string id);
    }
}