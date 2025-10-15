using ClothingOrderAndStockManagement.Domain.Entities.Customers;
using ClothingOrderAndStockManagement.Domain.Entities.Account;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<Users>> GetAllAsync();
        Task<Users?> GetByIdAsync(string id);
        Task CreateAsync(Users user, string password);
        Task UpdateAsync(Users user);
        Task DeleteAsync(Users user);
        IQueryable<Users> Query();
    }
}
