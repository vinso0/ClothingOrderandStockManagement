using ClothingOrderAndStockManagement.Application.Dtos.Packages;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Products;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Application.Services
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IItemRepository _itemRepository;

        public PackageService(IPackageRepository packageRepository, IItemRepository itemRepository)
        {
            _packageRepository = packageRepository;
            _itemRepository = itemRepository;
        }

        public async Task<Result<PaginatedList<PackageDto>>> GetPackagesAsync(string searchString, int pageIndex, int pageSize)
        {
            try
            {
                var query = _packageRepository.Query();

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    query = query.Where(p =>
                        p.PackageName.Contains(searchString) ||
                        (p.Description != null && p.Description.Contains(searchString)));
                }

                var dtoQuery = query.Select(p => new PackageDto
                {
                    PackagesId = p.PackagesId,
                    PackageName = p.PackageName,
                    Description = p.Description,
                    Price = p.Price
                });

                var paginatedList = await PaginatedList<PackageDto>.CreateAsync(dtoQuery, pageIndex, pageSize);
                return Result.Ok(paginatedList);
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
                var packages = await _packageRepository.GetAllAsync();
                return packages.Select(p => new PackageDto
                {
                    PackagesId = p.PackagesId,
                    PackageName = p.PackageName,
                    Description = p.Description,
                    Price = p.Price
                }).ToList();
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
                var package = await _packageRepository.GetByIdAsync(id);
                if (package == null)
                    return Result.Fail<PackageDto>("Package not found.");

                var dto = new PackageDto
                {
                    PackagesId = package.PackagesId,
                    PackageName = package.PackageName,
                    Description = package.Description,
                    Price = package.Price
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
                var package = await _packageRepository.GetByIdAsync(id);
                if (package == null)
                    return Result.Fail<PackageDetailDto>("Package not found.");

                var dto = new PackageDetailDto
                {
                    PackagesId = package.PackagesId,
                    PackageName = package.PackageName,
                    Description = package.Description,
                    Price = package.Price,
                    PackageItems = package.PackageItems.Select(pi => new PackageItemDto
                    {
                        PackageItemId = pi.PackageItemId,
                        ItemId = pi.ItemId,
                        ItemQuantity = pi.ItemQuantity,
                        ItemName = pi.Item.ItemCategory.ItemCategoryType,
                        Size = pi.Item.Size,
                        Color = pi.Item.Color
                    }).ToList()
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
                // Check item quantities before creating package
                foreach (var packageItem in packageDto.PackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(packageItem.ItemId);
                    if (item == null)
                        return Result.Fail($"Item with ID {packageItem.ItemId} not found.");

                    if (item.Quantity < packageItem.ItemQuantity)
                        return Result.Fail($"Insufficient quantity for item {item.ItemCategory.ItemCategoryType}. Available: {item.Quantity}, Required: {packageItem.ItemQuantity}");
                }

                // Create package
                var newPackage = new Package
                {
                    PackageName = packageDto.PackageName,
                    Description = packageDto.Description,
                    Price = packageDto.Price,
                    PackageItems = packageDto.PackageItems.Select(pi => new PackageItem
                    {
                        ItemId = pi.ItemId,
                        ItemQuantity = pi.ItemQuantity
                    }).ToList()
                };

                await _packageRepository.AddAsync(newPackage);

                // Deduct item quantities
                foreach (var packageItem in packageDto.PackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(packageItem.ItemId);
                    if (item != null) // Ensure item is not null before accessing its properties
                    {
                        item.Quantity -= packageItem.ItemQuantity;
                        await _itemRepository.UpdateAsync(item);
                    }
                }

                await _packageRepository.SaveChangesAsync();
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
                var existingPackage = await _packageRepository.GetByIdAsync(packageDto.PackagesId);
                if (existingPackage == null)
                    return Result.Fail("Package not found.");

                // First, restore quantities from the old package
                foreach (var oldItem in existingPackage.PackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(oldItem.ItemId);
                    if (item != null)
                    {
                        item.Quantity += oldItem.ItemQuantity;
                        await _itemRepository.UpdateAsync(item);
                    }
                }

                // Check quantities for new package items
                foreach (var packageItem in packageDto.PackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(packageItem.ItemId);
                    if (item == null)
                        return Result.Fail($"Item with ID {packageItem.ItemId} not found.");

                    if (item.Quantity < packageItem.ItemQuantity)
                        return Result.Fail($"Insufficient quantity for item {item.ItemCategory.ItemCategoryType}. Available: {item.Quantity}, Required: {packageItem.ItemQuantity}");
                }

                // Update package details
                existingPackage.PackageName = packageDto.PackageName;
                existingPackage.Description = packageDto.Description;
                existingPackage.Price = packageDto.Price;

                // Clear and add new package items
                existingPackage.PackageItems.Clear();
                existingPackage.PackageItems = packageDto.PackageItems.Select(pi => new PackageItem
                {
                    PackagesId = existingPackage.PackagesId,
                    ItemId = pi.ItemId,
                    ItemQuantity = pi.ItemQuantity
                }).ToList();

                // Deduct quantities for new package items
                foreach (var packageItem in packageDto.PackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(packageItem.ItemId);
                    if (item != null)
                    {
                        item.Quantity -= packageItem.ItemQuantity;
                        await _itemRepository.UpdateAsync(item);
                    }
                }

                await _packageRepository.UpdateAsync(existingPackage);
                await _packageRepository.SaveChangesAsync();

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

                // Restore item quantities when deleting package
                foreach (var packageItem in package.PackageItems)
                {
                    var item = await _itemRepository.GetByIdAsync(packageItem.ItemId);
                    if (item != null) // Ensure item is not null before accessing its properties
                    {
                        item.Quantity += packageItem.ItemQuantity;
                        await _itemRepository.UpdateAsync(item);
                    }
                }

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
                var packages = await _packageRepository.GetAvailablePackagesAsync();
                return packages.Select(p => new PackageDto
                {
                    PackagesId = p.PackagesId,
                    PackageName = p.PackageName,
                    Description = p.Description,
                    Price = p.Price
                }).ToList();
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
                    Price = p.Price
                }).ToList();
            }
            catch
            {
                return Enumerable.Empty<PackageDto>();
            }
        }
    }
}
