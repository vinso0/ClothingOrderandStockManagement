using ClothingOrderAndStockManagement.Domain.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Customers;
using ClothingOrderAndStockManagement.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Application.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IApplicationDbContext _context;

        public CustomerRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerInfo>> GetAllAsync()
        {
            return await _context.Set<CustomerInfo>().ToListAsync();
        }

        public async Task<CustomerInfo?> GetByIdAsync(int id)
        {
            return await _context.Set<CustomerInfo>().FindAsync(id);
        }

        public async Task<CustomerInfo?> GetCustomerByNameAndContactNumberAsync(string name, string contactNumber)
        {
            return await _context.Set<CustomerInfo>()
                .FirstOrDefaultAsync(c => c.CustomerName == name && c.ContactNumber == contactNumber);
        }

        public async Task AddAsync(CustomerInfo customer)
        {
            await _context.Set<CustomerInfo>().AddAsync(customer);
        }

        public async Task UpdateAsync(CustomerInfo customer)
        {
            _context.Set<CustomerInfo>().Update(customer);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _context.Set<CustomerInfo>().FindAsync(id);
            if (customer != null)
            {
                _context.Set<CustomerInfo>().Remove(customer);
            }
        }

        public IQueryable<CustomerInfo> Query()
        {
            return _context.Set<CustomerInfo>().AsQueryable();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
