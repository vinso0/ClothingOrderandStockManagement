
using FluentResults;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<int> CalculatePackageAvailabilityAsync(int packageId);
        Task<Result> UpdatePackageQuantityAsync(int packageId);
        Task<Result> UpdateAllPackageQuantitiesAsync();
        Task<bool> ValidatePackageAvailabilityAsync(int packageId, int requestedQuantity);
        Task<Result> ReservePackageQuantityAsync(int packageId, int quantity);
        Task<Result> ReleasePackageQuantityAsync(int packageId, int quantity);
    }
}
