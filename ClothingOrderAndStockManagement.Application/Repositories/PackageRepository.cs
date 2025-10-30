
using ClothingOrderAndStockManagement.Domain.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Products;
using ClothingOrderAndStockManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Application.Repositories
{
    public class PackageRepository : IPackageRepository
    {
        private readonly ApplicationDbContext _context;

        public PackageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all packages from the database
        /// </summary>
        public async Task<IEnumerable<Package>> GetAllAsync()
        {
            return await _context.Packages
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.Item)
                .OrderBy(p => p.PackageName)
                .ToListAsync();
        }

        /// <summary>
        /// Get a single package by its ID
        /// </summary>
        public async Task<Package?> GetByIdAsync(int id)
        {
            return await _context.Packages
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.Item)
                .FirstOrDefaultAsync(p => p.PackagesId == id);
        }

        /// <summary>
        /// Get packages by name (search)
        /// </summary>
        public async Task<IEnumerable<Package>> GetByNameAsync(string name)
        {
            return await _context.Packages
                .Where(p => p.PackageName.Contains(name))
                .OrderBy(p => p.PackageName)
                .ToListAsync();
        }

        /// <summary>
        /// Add a new package to the database
        /// </summary>
        public async Task AddAsync(Package package)
        {
            await _context.Packages.AddAsync(package);
        }

        /// <summary>
        /// Update an existing package
        /// </summary>
        public async Task UpdateAsync(Package package)
        {
            _context.Packages.Update(package);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Delete a package by ID
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var package = await _context.Packages
                .Include(p => p.PackageItems)
                .FirstOrDefaultAsync(p => p.PackagesId == id);

            if (package != null)
            {
                // Remove related package items first
                _context.PackageItems.RemoveRange(package.PackageItems);
                _context.Packages.Remove(package);
            }
        }

        /// <summary>
        /// Get queryable collection of packages for advanced filtering
        /// </summary>
        public IQueryable<Package> Query()
        {
            return _context.Packages.AsQueryable();
        }

        /// <summary>
        /// Save changes to the database
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Check if a package exists by ID
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Packages.AnyAsync(p => p.PackagesId == id);
        }

        /// <summary>
        /// Get available packages (all packages for now, can be filtered based on business rules)
        /// </summary>
        public async Task<IEnumerable<Package>> GetAvailablePackagesAsync()
        {
            // You can add business logic here, e.g., only return packages with available stock
            return await _context.Packages
                .Where(p => p.Price > 0) // Example: only packages with price set
                .OrderBy(p => p.PackageName)
                .ToListAsync();
        }
    }
}