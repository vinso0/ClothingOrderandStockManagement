using ClothingOrderAndStockManagement.Domain.Entities.Account;
using Microsoft.AspNetCore.Identity;

namespace ClothingOrderAndStockManagement.Domain.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Success, Users? User)> PasswordSignInAsync(string usernameOrEmail, string password, bool rememberMe);
        Task<Users?> GetByEmailAsync(string email);
        Task<Users?> GetByUsernameAsync(string username);
        Task<IList<string>> GetRolesAsync(Users user);
        Task<IdentityResult> ResetPasswordAsync(Users user, string token, string newPassword);
        Task SignOutAsync();
    }
}
