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
            try
            {
                // Validate items exist
                foreach (var packageItem in packageDto.PackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(packageItem.ItemId);
                    if (item == null)
                        return Result.Fail($"Item with ID {packageItem.ItemId} not found.");
                    if (packageItem.ItemQuantity <= 0)
                        return Result.Fail("ItemQuantity per package must be greater than zero.");
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

                // Optionally recompute QuantityAvailable from items if using inventory service
                // await _inventoryService.UpdatePackageQuantityAsync(newPackage.PackagesId);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Result> UpdatePackageAsync(UpdatePackageDto packageDto)
        {
            try
            {
                var existing = await _packageRepository.Query()
                    .Include(p => p.PackageItems)
                    .FirstOrDefaultAsync(p => p.PackagesId == packageDto.PackagesId);

                if (existing == null) return Result.Fail("Package not found.");

                // Validate new composition
                foreach (var packageItem in packageDto.PackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(packageItem.ItemId);
                    if (item == null)
                        return Result.Fail($"Item with ID {packageItem.ItemId} not found.");
                    if (packageItem.ItemQuantity <= 0)
                        return Result.Fail("ItemQuantity per package must be greater than zero.");
                }

                // Update basic fields
                existing.PackageName = packageDto.PackageName;
                existing.Description = packageDto.Description;
                existing.Price = packageDto.Price;
                existing.QuantityAvailable = packageDto.QuantityAvailable;

                // Replace composition
                existing.PackageItems.Clear();
                existing.PackageItems = packageDto.PackageItems.Select(pi => new PackageItem
                {
                    PackagesId = existing.PackagesId,
                    ItemId = pi.ItemId,
                    ItemQuantity = pi.ItemQuantity
                }).ToList();

                await _packageRepository.UpdateAsync(existing);
                await _packageRepository.SaveChangesAsync();

                // Optionally recompute availability
                // await _inventoryService.UpdatePackageQuantityAsync(existing.PackagesId);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Result> DeletePackageAsync(int id)
        {
            try
            {
                var package = await _packageRepository.GetByIdAsync(id);
                if (package == null)
                    return Result.Fail("Package not found.");

                await _packageRepository.DeleteAsync(id);
                await _packageRepository.SaveChangesAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
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
