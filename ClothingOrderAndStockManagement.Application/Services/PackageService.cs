using System.Transactions;
using ClothingOrderAndStockManagement.Application.Dtos.Packages;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Products;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Application.Services
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IInventoryService _inventoryService;

        public PackageService(
            IPackageRepository packageRepository,
            IItemRepository itemRepository,
            IInventoryService inventoryService)
        {
            _packageRepository = packageRepository;
            _itemRepository = itemRepository;
            _inventoryService = inventoryService;
        }

        public async Task<Result<PaginatedList<PackageDto>>> GetPackagesAsync(string searchString, int pageIndex, int pageSize)
        {
            try
            {
                // 1) Filter first on the base query (fast and type-stable)
                var baseQuery = _packageRepository.Query();

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PackageName.Contains(searchString) ||
                        (p.Description != null && p.Description.Contains(searchString)));
                }

                // 2) Total count BEFORE includes/projection
                var totalCount = await baseQuery.CountAsync();

                // 3) Build the include + projection query for the current page only
                var pageQuery = baseQuery
                    .Include(p => p.PackageItems)
                        .ThenInclude(pi => pi.Item)
                            .ThenInclude(i => i.ItemCategory)
                    .OrderBy(p => p.PackagesId) // deterministic ordering for pagination
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PackageDto
                    {
                        PackagesId = p.PackagesId,
                        PackageName = p.PackageName,
                        Description = p.Description,
                        Price = p.Price,
                        QuantityAvailable = p.QuantityAvailable,
                        PackageItems = p.PackageItems.Select(pi => new PackageItemDto
                        {
                            PackageItemId = pi.PackageItemId,
                            ItemId = pi.ItemId,
                            ItemQuantity = pi.ItemQuantity,
                            ItemName = pi.Item != null && pi.Item.ItemCategory != null
                                ? pi.Item.ItemCategory.ItemCategoryType
                                : null,
                            Size = pi.Item != null ? pi.Item.Size : null,
                            Color = pi.Item != null ? pi.Item.Color : null
                        }).ToList()
                    });

                var items = await pageQuery.ToListAsync();

                // 4) Construct PaginatedList manually (bypasses CreateAsync translation pitfalls)
                var paginated = new PaginatedList<PackageDto>(items, totalCount, pageIndex, pageSize);
                return Result.Ok(paginated);
            }
            catch (Exception ex)
            {
                return Result.Fail<PaginatedList<PackageDto>>(ex.Message);
            }
        }

        public async Task<IEnumerable<PackageDto>> GetAllPackagesAsync()
        {
            try
            {
                var packages = await _packageRepository.Query()
                    .Include(p => p.PackageItems)
                        .ThenInclude(pi => pi.Item)
                            .ThenInclude(i => i.ItemCategory)
                    .Select(p => new PackageDto
                    {
                        PackagesId = p.PackagesId,
                        PackageName = p.PackageName,
                        Description = p.Description,
                        Price = p.Price,
                        QuantityAvailable = p.QuantityAvailable,
                        PackageItems = p.PackageItems.Select(pi => new PackageItemDto
                        {
                            PackageItemId = pi.PackageItemId,
                            ItemId = pi.ItemId,
                            ItemQuantity = pi.ItemQuantity,
                            ItemName = pi.Item != null && pi.Item.ItemCategory != null
                                ? pi.Item.ItemCategory.ItemCategoryType
                                : null,
                            Size = pi.Item != null ? pi.Item.Size : null,
                            Color = pi.Item != null ? pi.Item.Color : null
                        }).ToList()
                    })
                    .ToListAsync();

                return packages;
            }
            catch
            {
                return Enumerable.Empty<PackageDto>();
            }
        }

        public async Task<Result<PackageDto>> GetPackageByIdAsync(int id)
        {
            try
            {
                var package = await _packageRepository.Query()
                    .Include(p => p.PackageItems)
                        .ThenInclude(pi => pi.Item)
                            .ThenInclude(i => i.ItemCategory)
                    .FirstOrDefaultAsync(p => p.PackagesId == id);

                if (package == null)
                    return Result.Fail<PackageDto>("Package not found.");

                var dto = new PackageDto
                {
                    PackagesId = package.PackagesId,
                    PackageName = package.PackageName,
                    Description = package.Description,
                    Price = package.Price,
                    QuantityAvailable = package.QuantityAvailable,
                    PackageItems = package.PackageItems.Select(pi => new PackageItemDto
                    {
                        PackageItemId = pi.PackageItemId,
                        ItemId = pi.ItemId,
                        ItemQuantity = pi.ItemQuantity,
                        ItemName = pi.Item != null && pi.Item.ItemCategory != null
                            ? pi.Item.ItemCategory.ItemCategoryType
                            : null,
                        Size = pi.Item != null ? pi.Item.Size : null,
                        Color = pi.Item != null ? pi.Item.Color : null
                    }).ToList()
                };

                return Result.Ok(dto);
            }
            catch (Exception ex)
            {
                return Result.Fail<PackageDto>(ex.Message);
            }
        }

        public async Task<Result<PackageDetailDto>> GetPackageDetailsAsync(int id)
        {
            try
            {
                var packageResult = await GetPackageByIdAsync(id);
                if (!packageResult.IsSuccess)
                    return Result.Fail<PackageDetailDto>(packageResult.Errors.First().Message);

                var package = packageResult.Value;
                var dto = new PackageDetailDto
                {
                    PackagesId = package.PackagesId,
                    PackageName = package.PackageName,
                    Description = package.Description,
                    Price = package.Price,
                    QuantityAvailable = package.QuantityAvailable,
                    PackageItems = package.PackageItems
                };

                return Result.Ok(dto);
            }
            catch (Exception ex)
            {
                return Result.Fail<PackageDetailDto>(ex.Message);
            }
        }

        public async Task<Result> AddPackageAsync(CreatePackageDto packageDto)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                // 1. Validate items exist and have sufficient stock
                foreach (var packageItem in packageDto.PackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(packageItem.ItemId);
                    if (item == null)
                        return Result.Fail($"Item with ID {packageItem.ItemId} not found.");

                    if (packageItem.ItemQuantity <= 0)
                        return Result.Fail("ItemQuantity per package must be greater than zero.");

                    // Use Item ID instead of ItemCategoryType
                    var totalNeeded = packageItem.ItemQuantity * packageDto.QuantityAvailable;
                    if (item.Quantity < totalNeeded)
                        return Result.Fail($"Insufficient stock for Item ID {item.ItemId}. Required: {totalNeeded}, Available: {item.Quantity}");
                }

                var newPackage = new Package
                {
                    PackageName = packageDto.PackageName,
                    Description = packageDto.Description,
                    Price = packageDto.Price,
                    QuantityAvailable = packageDto.QuantityAvailable,
                    PackageItems = packageDto.PackageItems.Select(pi => new PackageItem
                    {
                        ItemId = pi.ItemId,
                        ItemQuantity = pi.ItemQuantity
                    }).ToList()
                };

                await _packageRepository.AddAsync(newPackage);
                await _packageRepository.SaveChangesAsync();

                // 3. Deduct items from inventory
                foreach (var packageItem in packageDto.PackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(packageItem.ItemId);
                    var totalToDeduct = packageItem.ItemQuantity * packageDto.QuantityAvailable;
                    if (item != null)
                    {
                        item.Quantity -= totalToDeduct;
                    }
                    else
                    {
                        return Result.Fail($"Item with ID {packageItem.ItemId} not found during inventory deduction.");
                    }
                    await _itemRepository.UpdateAsync(item);
                }

                await _itemRepository.SaveChangesAsync();
                scope.Complete();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }


        public async Task<Result> UpdatePackageAsync(UpdatePackageDto packageDto)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                // 1. Get the existing package with its items
                var existing = await _packageRepository.Query()
                    .Include(p => p.PackageItems)
                    .FirstOrDefaultAsync(p => p.PackagesId == packageDto.PackagesId);

                if (existing == null)
                    return Result.Fail("Package not found.");

                // 2. Store original quantities for rollback calculation
                var originalQuantityAvailable = existing.QuantityAvailable;
                var originalPackageItems = existing.PackageItems.ToList();

                // 3. Restore items from the old package composition back to inventory
                foreach (var oldItem in originalPackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(oldItem.ItemId);
                    if (item != null)
                    {
                        var totalToRestore = oldItem.ItemQuantity * originalQuantityAvailable;
                        item.Quantity += totalToRestore;
                        await _itemRepository.UpdateAsync(item);
                    }
                }
                await _itemRepository.SaveChangesAsync();

                // 4. Validate new composition and check if we have enough stock
                foreach (var packageItem in packageDto.PackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(packageItem.ItemId);
                    if (item == null)
                        return Result.Fail($"Item with ID {packageItem.ItemId} not found.");

                    if (packageItem.ItemQuantity <= 0)
                        return Result.Fail("ItemQuantity per package must be greater than zero.");

                    // Calculate total needed for all packages of this type
                    var totalNeeded = packageItem.ItemQuantity * packageDto.QuantityAvailable;
                    if (item.Quantity < totalNeeded)
                        return Result.Fail($"Insufficient stock for Item ID {item.ItemId} ({item.Size} {item.Color}). Required: {totalNeeded}, Available: {item.Quantity}");
                }

                // 5. Update package basic fields
                existing.PackageName = packageDto.PackageName;
                existing.Description = packageDto.Description;
                existing.Price = packageDto.Price;
                existing.QuantityAvailable = packageDto.QuantityAvailable;

                // 6. Update package items relationship (safe approach)
                // Remove items that are no longer in the package
                var existingItems = existing.PackageItems.ToList();
                foreach (var existingItem in existingItems)
                {
                    if (!packageDto.PackageItems.Any(pi => pi.ItemId == existingItem.ItemId))
                    {
                        existing.PackageItems.Remove(existingItem);
                    }
                }

                // Update existing items or add new ones
                foreach (var newItem in packageDto.PackageItems)
                {
                    var existingItem = existing.PackageItems
                        .FirstOrDefault(pi => pi.ItemId == newItem.ItemId);

                    if (existingItem != null)
                    {
                        // Update quantity for existing item
                        existingItem.ItemQuantity = newItem.ItemQuantity;
                    }
                    else
                    {
                        // Add new item to package
                        existing.PackageItems.Add(new PackageItem
                        {
                            PackagesId = existing.PackagesId,
                            ItemId = newItem.ItemId,
                            ItemQuantity = newItem.ItemQuantity
                        });
                    }
                }

                await _packageRepository.UpdateAsync(existing);
                await _packageRepository.SaveChangesAsync();

                // 7. Deduct items from inventory for new composition
                foreach (var packageItem in packageDto.PackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(packageItem.ItemId);
                    if (item != null)
                    {
                        var totalToDeduct = packageItem.ItemQuantity * packageDto.QuantityAvailable;
                        item.Quantity -= totalToDeduct;
                        await _itemRepository.UpdateAsync(item);
                    }
                }

                await _itemRepository.SaveChangesAsync();
                scope.Complete(); // Commit transaction
                return Result.Ok();
            }
            catch (Exception ex)
            {
                // Transaction automatically rolls back if scope.Complete() is not called
                return Result.Fail($"Failed to update package: {ex.Message}");
            }
        }


        public async Task<Result> DeletePackageAsync(int id)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                // 1. Get package with its items
                var package = await _packageRepository.Query()
                    .Include(p => p.PackageItems)
                    .FirstOrDefaultAsync(p => p.PackagesId == id);

                if (package == null)
                    return Result.Fail("Package not found.");

                // 2. Restore items to inventory before deleting package
                foreach (var packageItem in package.PackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(packageItem.ItemId);
                    if (item != null)
                    {
                        var totalToRestore = packageItem.ItemQuantity * package.QuantityAvailable;
                        item.Quantity += totalToRestore;
                        await _itemRepository.UpdateAsync(item);
                    }
                }

                // 3. Delete the package
                await _packageRepository.DeleteAsync(id);

                // 4. Save all changes
                await _itemRepository.SaveChangesAsync();
                await _packageRepository.SaveChangesAsync();

                scope.Complete(); // Commit transaction
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to delete package: {ex.Message}");
            }
        }


        public async Task<IEnumerable<PackageDto>> GetAvailablePackagesAsync()
        {
            try
            {
                var packages = await _packageRepository.Query()
                    .Where(p => p.QuantityAvailable > 0)
                    .Include(p => p.PackageItems)
                        .ThenInclude(pi => pi.Item)
                            .ThenInclude(i => i.ItemCategory)
                    .Select(p => new PackageDto
                    {
                        PackagesId = p.PackagesId,
                        PackageName = p.PackageName,
                        Description = p.Description,
                        Price = p.Price,
                        QuantityAvailable = p.QuantityAvailable,
                        PackageItems = p.PackageItems.Select(pi => new PackageItemDto
                        {
                            PackageItemId = pi.PackageItemId,
                            ItemId = pi.ItemId,
                            ItemQuantity = pi.ItemQuantity,
                            ItemName = pi.Item != null && pi.Item.ItemCategory != null
                                ? pi.Item.ItemCategory.ItemCategoryType
                                : null,
                            Size = pi.Item != null ? pi.Item.Size : null,
                            Color = pi.Item != null ? pi.Item.Color : null
                        }).ToList()
                    })
                    .ToListAsync();

                return packages;
            }
            catch
            {
                return Enumerable.Empty<PackageDto>();
            }
        }

        public async Task<IEnumerable<PackageDto>> SearchPackagesByNameAsync(string name)
        {
            try
            {
                var packages = await _packageRepository.GetByNameAsync(name);
                return packages.Select(p => new PackageDto
                {
                    PackagesId = p.PackagesId,
                    PackageName = p.PackageName,
                    Description = p.Description,
                    Price = p.Price,
                    QuantityAvailable = p.QuantityAvailable
                }).ToList();
            }
            catch
            {
                return Enumerable.Empty<PackageDto>();
            }
        }
    }
}
