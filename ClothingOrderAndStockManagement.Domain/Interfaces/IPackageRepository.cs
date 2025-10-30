// File Location: ClothingOrderAndStockManagement.Application/Interfaces/IPackageRepository.cs

using ClothingOrderAndStockManagement.Domain.Entities.Products;

namespace ClothingOrderAndStockManagement.Domain.Interfaces
{
    public interface IPackageRepository
    {
        /// <summary>
        /// Get all packages from the database
        /// </summary>
        Task<IEnumerable<Package>> GetAllAsync();

        /// <summary>
        /// Get a single package by its ID
        /// </summary>
        Task<Package?> GetByIdAsync(int id);

        /// <summary>
        /// Get packages by name (search)
        /// </summary>
        Task<IEnumerable<Package>> GetByNameAsync(string name);

        /// <summary>
        /// Add a new package to the database
        /// </summary>
        Task AddAsync(Package package);

        /// <summary>
        /// Update an existing package
        /// </summary>
        Task UpdateAsync(Package package);

        /// <summary>
        /// Delete a package by ID
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Get queryable collection of packages for advanced filtering
        /// </summary>
        IQueryable<Package> Query();

        /// <summary>
        /// Save changes to the database
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Check if a package exists by ID
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Get available packages (packages with stock or active packages)
        /// </summary>
        Task<IEnumerable<Package>> GetAvailablePackagesAsync();
    }
}