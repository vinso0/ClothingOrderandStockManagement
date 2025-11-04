
using ClothingOrderAndStockManagement.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClothingOrderAndStockManagement.Domain.Interfaces
{
    public interface IPackageRepository
    {
        Task<IEnumerable<Package>> GetAllAsync();
        Task<Package?> GetByIdAsync(int id);
        Task<IEnumerable<Package>> GetByNameAsync(string name);
        Task AddAsync(Package package);
        Task UpdateAsync(Package package);
        Task DeleteAsync(int id);
        IQueryable<Package> Query();
        Task SaveChangesAsync();
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Package>> GetAvailablePackagesAsync();
    }
}