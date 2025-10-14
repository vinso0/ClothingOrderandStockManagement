using ClothingOrderAndStockManagement.Domain.Entities.Customers;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetCustomerByNameAndContactNumberAsync(string name, string contactNumber);
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);
        IQueryable<Customer> Query();
        Task SaveChangesAsync();
    }
}
