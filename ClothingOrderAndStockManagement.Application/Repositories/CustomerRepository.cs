using ClothingOrderAndStockManagement.Domain.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Customers;
using ClothingOrderAndStockManagement.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Application.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerInfo>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<CustomerInfo?> GetByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<CustomerInfo?> GetCustomerByNameAndContactNumberAsync(string name, string contactNumber)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerName == name && c.ContactNumber == contactNumber);
        }

        public async Task AddAsync(CustomerInfo customer)
        {
            await _context.Customers.AddAsync(customer);
        }

        public async Task UpdateAsync(CustomerInfo customer)
        {
            _context.Customers.Update(customer);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }
        }

        public IQueryable<CustomerInfo> Query()
        {
            return _context.Customers.AsQueryable();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
