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

        public PackageService(IPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
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
                var package = await _packageRepository.GetByIdAsync(packageDto.PackagesId);
                if (package == null)
                    return Result.Fail("Package not found.");

                package.PackageName = packageDto.PackageName;
                package.Description = packageDto.Description;
                package.Price = packageDto.Price;

                // Update package items
                package.PackageItems.Clear();
                package.PackageItems = packageDto.PackageItems.Select(pi => new PackageItem
                {
                    PackagesId = package.PackagesId,
                    ItemId = pi.ItemId,
                    ItemQuantity = pi.ItemQuantity
                }).ToList();

                await _packageRepository.UpdateAsync(package);
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