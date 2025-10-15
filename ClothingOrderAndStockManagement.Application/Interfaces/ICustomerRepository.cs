using ClothingOrderAndStockManagement.Domain.Entities.Customers;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerInfo>> GetAllAsync();
        Task<CustomerInfo?> GetByIdAsync(int id);
        Task<CustomerInfo?> GetCustomerByNameAndContactNumberAsync(string name, string contactNumber);
        Task AddAsync(CustomerInfo customer);
        Task UpdateAsync(CustomerInfo customer);
        Task DeleteAsync(int id);
        IQueryable<CustomerInfo> Query();
        Task SaveChangesAsync();
    }
}
