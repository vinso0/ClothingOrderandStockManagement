// File Location: ClothingOrderAndStockManagement.Application/Interfaces/IPackageService.cs

using ClothingOrderAndStockManagement.Application.Dtos.Packages;
using ClothingOrderAndStockManagement.Application.Helpers;
using FluentResults;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface IPackageService
    {
        Task<Result<PaginatedList<PackageDto>>> GetPackagesAsync(string searchString, int pageIndex, int pageSize);
        Task<IEnumerable<PackageDto>> GetAllPackagesAsync();
        Task<Result<PackageDto>> GetPackageByIdAsync(int id);
        Task<Result<PackageDetailDto>> GetPackageDetailsAsync(int id);
        Task<Result> AddPackageAsync(CreatePackageDto packageDto);
        Task<Result> UpdatePackageAsync(UpdatePackageDto packageDto);
        Task<Result> DeletePackageAsync(int id);
        Task<IEnumerable<PackageDto>> GetAvailablePackagesAsync();
        Task<IEnumerable<PackageDto>> SearchPackagesByNameAsync(string name);
    }
}
