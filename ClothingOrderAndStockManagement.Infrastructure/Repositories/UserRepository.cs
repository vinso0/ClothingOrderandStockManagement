using ClothingOrderAndStockManagement.Domain.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<Users> _userManager;

        public UserRepository(UserManager<Users> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<Users>> GetAllAsync()
            => await _userManager.Users.ToListAsync();

        public async Task<Users?> GetByIdAsync(string id)
            => await _userManager.FindByIdAsync(id);

        public async Task CreateAsync(Users user, string password)
            => await _userManager.CreateAsync(user, password);

        public async Task UpdateAsync(Users user)
            => await _userManager.UpdateAsync(user);

        public async Task DeleteAsync(Users user)
            => await _userManager.DeleteAsync(user);

        public IQueryable<Users> Query()
        {
            return _userManager.Users.AsQueryable();
        }
    }

}
