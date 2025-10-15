using ClothingOrderAndStockManagement.Application.Dtos.Users;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Account;
using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace ClothingOrderAndStockManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<Users> _userManager;

        public UserService(UserManager<Users> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<PaginatedList<UserDto>>> GetUsersAsync(string searchString, int pageIndex, int pageSize)
        {
            try
            {
                var query = _userManager.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    query = query.Where(u =>
                        u.UserName!.Contains(searchString) ||
                        u.Email!.Contains(searchString));
                }

                var users = await PaginatedList<Users>.CreateAsync(query, pageIndex, pageSize);

                var dtoList = new List<UserDto>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    dtoList.Add(new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName!,
                        Email = user.Email!,
                        Role = roles.FirstOrDefault() ?? "No Role"
                    });
                }

                var paginated = new PaginatedList<UserDto>(dtoList, users.TotalCount, pageIndex, pageSize);
                return Result.Ok(paginated);
            }
            catch (Exception ex)
            {
                return Result.Fail<PaginatedList<UserDto>>(ex.Message);
            }
        }

        public async Task<Result<UserDto>> GetUserByIdAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return Result.Fail<UserDto>("User not found.");

                var roles = await _userManager.GetRolesAsync(user);

                var dto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    Role = roles.FirstOrDefault() ?? "No Role"
                };

                return Result.Ok(dto);
            }
            catch (Exception ex)
            {
                return Result.Fail<UserDto>(ex.Message);
            }
        }

        public async Task<Result> CreateUserAsync(CreateUserDto dto)
        {
            try
            {
                var user = new Users
                {
                    UserName = dto.UserName,
                    Email = dto.Email
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errorMsg = string.Join("; ", result.Errors.Select(e => e.Description));
                    return Result.Fail(errorMsg);
                }

                if (!string.IsNullOrEmpty(dto.Role))
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
                    if (!roleResult.Succeeded)
                    {
                        var errorMsg = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                        return Result.Fail(errorMsg);
                    }
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Result> UpdateUserAsync(EditUserDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(dto.Id);
                if (user == null)
                    return Result.Fail("User not found.");

                user.UserName = dto.UserName;
                user.Email = dto.Email;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errorMsg = string.Join("; ", result.Errors.Select(e => e.Description));
                    return Result.Fail(errorMsg);
                }

                if (!string.IsNullOrWhiteSpace(dto.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var pwdResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);

                    if (!pwdResult.Succeeded)
                    {
                        var errorMsg = string.Join("; ", pwdResult.Errors.Select(e => e.Description));
                        return Result.Fail(errorMsg);
                    }
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        var errorMsg = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                        return Result.Fail(errorMsg);
                    }
                }
                if (!string.IsNullOrEmpty(dto.Role))
                {
                    var addRoleResult = await _userManager.AddToRoleAsync(user, dto.Role);
                    if (!addRoleResult.Succeeded)
                    {
                        var errorMsg = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                        return Result.Fail(errorMsg);
                    }
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Result> DeleteUserAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return Result.Fail("User not found.");

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errorMsg = string.Join("; ", result.Errors.Select(e => e.Description));
                    return Result.Fail(errorMsg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Task<List<string>> GetUserRoleAsync()
        {
            throw new NotImplementedException();
        }
    }
}