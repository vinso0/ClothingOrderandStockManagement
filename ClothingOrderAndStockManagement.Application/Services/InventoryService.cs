using ClothingOrderAndStockManagement.Domain.Interfaces;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using ClothingOrderAndStockManagement.Application.Interfaces;

namespace ClothingOrderAndStockManagement.Application.Services
{

    public class InventoryService : IInventoryService
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IItemRepository _itemRepository;

        public InventoryService(IPackageRepository packageRepository, IItemRepository itemRepository)
        {
            _packageRepository = packageRepository;
            _itemRepository = itemRepository;
        }

        public async Task<int> CalculatePackageAvailabilityAsync(int packageId)
        {
            var package = await _packageRepository.Query()
                .Include(p => p.PackageItems)
                .ThenInclude(pi => pi.Item)
                .FirstOrDefaultAsync(p => p.PackagesId == packageId);

            if (package == null || package.PackageItems.Count == 0) return 0;

            int minPossible = int.MaxValue;

            foreach (var pi in package.PackageItems)
            {
                // If any item is missing or has 0, availability is 0
                var itemQuantity = pi.Item?.Quantity ?? 0;
                if (pi.ItemQuantity <= 0) return 0;

                var possible = itemQuantity / pi.ItemQuantity;
                if (possible < minPossible) minPossible = possible;
            }

            return minPossible == int.MaxValue ? 0 : minPossible;
        }

        public async Task<Result> UpdatePackageQuantityAsync(int packageId)
        {
            var package = await _packageRepository.GetByIdAsync(packageId);
            if (package == null) return Result.Fail("Package not found.");

            package.QuantityAvailable = await CalculatePackageAvailabilityAsync(packageId);

            await _packageRepository.UpdateAsync(package);
            await _packageRepository.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> UpdateAllPackageQuantitiesAsync()
        {
            var packages = await _packageRepository.Query()
                .Select(p => p.PackagesId)
                .ToListAsync();

            foreach (var id in packages)
                await UpdatePackageQuantityAsync(id);

            return Result.Ok();
        }

        public async Task<bool> ValidatePackageAvailabilityAsync(int packageId, int requestedQuantity)
        {
            var package = await _packageRepository.GetByIdAsync(packageId);
            if (package == null) return false;
            // trust current stored value
            return package.QuantityAvailable >= requestedQuantity;
        }

        // Hard reservation: subtract from QuantityAvailable immediately
        public async Task<Result> ReservePackageQuantityAsync(int packageId, int quantity)
        {
            var package = await _packageRepository.GetByIdAsync(packageId);
            if (package == null) return Result.Fail("Package not found.");

            if (package.QuantityAvailable < quantity)
                return Result.Fail("Insufficient package quantity.");

            package.QuantityAvailable -= quantity;
            await _packageRepository.UpdateAsync(package);
            await _packageRepository.SaveChangesAsync();
            return Result.Ok();
        }

        // Release reservation (e.g., order canceled)
        public async Task<Result> ReleasePackageQuantityAsync(int packageId, int quantity)
        {
            var package = await _packageRepository.GetByIdAsync(packageId);
            if (package == null) return Result.Fail("Package not found.");

            package.QuantityAvailable += quantity;
            await _packageRepository.UpdateAsync(package);
            await _packageRepository.SaveChangesAsync();
            return Result.Ok();
        }
    }
}
