using ClothingOrderAndStockManagement.Application.Interfaces;
using Microsoft.AspNetCore.Identity; // Ensure this is included
using ClothingOrderAndStockManagement.Domain.Entities.Users;
using ClothingOrderAndStockManagement.Application.Services;

namespace ClothingOrderAndStockManagement.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly SignInManager<Users> _signInManager;
        private readonly UserManager<Users> _userManager;

        public AccountService(SignInManager<Users> signInManager,
                              UserManager<Users> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<(bool Success, Users? User)> PasswordSignInAsync(string usernameOrEmail, string password, bool rememberMe)
        {
            Users? user = usernameOrEmail.Contains("@")
                ? await _userManager.FindByEmailAsync(usernameOrEmail)
                : await _userManager.FindByNameAsync(usernameOrEmail);

            if (user == null || string.IsNullOrEmpty(user.UserName)) return (false, null);

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, rememberMe, lockoutOnFailure: false);
            return (result.Succeeded, result.Succeeded ? user : null);
        }

        public Task<Users?> GetByEmailAsync(string email) => _userManager.FindByEmailAsync(email);
        public Task<Users?> GetByUsernameAsync(string username) => _userManager.FindByNameAsync(username);
        public Task<IList<string>> GetRolesAsync(Users user) => _userManager.GetRolesAsync(user);
        public Task<IdentityResult> ResetPasswordAsync(Users user, string token, string newPassword) => _userManager.ResetPasswordAsync(user, token, newPassword);
        public Task SignOutAsync() => _signInManager.SignOutAsync();
    }
}
