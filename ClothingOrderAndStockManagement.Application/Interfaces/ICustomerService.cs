using ClothingOrderAndStockManagement.Application.Dtos.Customers;
using ClothingOrderAndStockManagement.Application.Helpers;
using FluentResults;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<Result<PaginatedList<CustomerDto>>> GetCustomersAsync(string searchString, int pageIndex, int pageSize);
        Task<Result<CustomerDto>> GetCustomerByIdAsync(int id);
        Task<Result> AddCustomerAsync(CustomerDto customerDto);
        Task<Result> UpdateCustomerAsync(CustomerDto customerDto);
        Task<Result> DeleteCustomerAsync(int id);
    }
}
