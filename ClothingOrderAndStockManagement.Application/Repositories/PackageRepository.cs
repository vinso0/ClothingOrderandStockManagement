using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Products;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClothingOrderAndStockManagement.Application.Repositories
{
    public class PackageRepository : IPackageRepository
    {
        private readonly IApplicationDbContext _context;

        public PackageRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        /// Get all packages from the database
        public async Task<IEnumerable<Package>> GetAllAsync()
        {
            return await _context.Set<Package>()
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.Item)
                .OrderBy(p => p.PackageName)
                .ToListAsync();
        }

        /// Get a single package by its ID
        public async Task<Package?> GetByIdAsync(int id)
        {
            return await _context.Set<Package>()
                .Include(p => p.PackageItems)
                    .ThenInclude(pi => pi.Item)
                .FirstOrDefaultAsync(p => p.PackagesId == id);
        }

        /// Get packages by name (search)
        public async Task<IEnumerable<Package>> GetByNameAsync(string name)
        {
            return await _context.Set<Package>()
                .Where(p => p.PackageName.Contains(name))
                .OrderBy(p => p.PackageName)
                .ToListAsync();
        }

        /// Add a new package to the database
        public async Task AddAsync(Package package)
        {
            await _context.Set<Package>().AddAsync(package);
        }

        /// Update an existing package
        public async Task UpdateAsync(Package package)
        {
            _context.Set<Package>().Update(package);
            await Task.CompletedTask;
        }

        /// Delete a package by ID
        public async Task DeleteAsync(int id)
        {
            var package = await _context.Set<Package>()
                .Include(p => p.PackageItems)
                .FirstOrDefaultAsync(p => p.PackagesId == id);

            if (package != null)
            {
                // Remove related package items first
                _context.Set<PackageItem>().RemoveRange(package.PackageItems);
                _context.Set<Package>().Remove(package);
            }
        }

        /// Get queryable collection of packages for advanced filtering
        public IQueryable<Package> Query()
        {
            return _context.Set<Package>().AsQueryable();
        }

        /// Save changes to the database
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// Check if a package exists by ID
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Set<Package>().AnyAsync(p => p.PackagesId == id);
        }

        /// Get available packages (all packages for now, can be filtered based on business rules)
        public async Task<IEnumerable<Package>> GetAvailablePackagesAsync()
        {
            // You can add business logic here, e.g., only return packages with available stock
            return await _context.Set<Package>()
                .Where(p => p.Price > 0) // Example: only packages with price set
                .OrderBy(p => p.PackageName)
                .ToListAsync();
        }
    }
}