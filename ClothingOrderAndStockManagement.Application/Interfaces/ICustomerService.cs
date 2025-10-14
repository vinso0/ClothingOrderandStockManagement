using ClothingOrderAndStockManagement.Application.Dtos.Customers;
using ClothingOrderAndStockManagement.Application.Helpers;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<PaginatedList<CustomerDto>> GetCustomersAsync(string searchString, int pageIndex, int pageSize);
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task AddCustomerAsync(CustomerDto customerDto);
        Task UpdateCustomerAsync(CustomerDto customerDto);
        Task DeleteCustomerAsync(int id);
    }
}
