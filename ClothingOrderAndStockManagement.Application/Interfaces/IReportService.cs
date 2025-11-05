using ClothingOrderAndStockManagement.Application.Dtos.Report;
using FluentResults;

namespace ClothingOrderAndStockManagement.Domain.Interfaces
{
    public interface IReportService
    {
        Task<Result<SystemReportDto>> GenerateSystemReportAsync(int daysWindowForReturns);
    }
}
