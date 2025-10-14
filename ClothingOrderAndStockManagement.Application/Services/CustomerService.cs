using ClothingOrderAndStockManagement.Application.Dtos.Customers;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Customers;

namespace ClothingOrderAndStockManagement.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<PaginatedList<CustomerDto>> GetCustomersAsync(string searchString, int pageIndex, int pageSize)
        {
            var query = _customerRepository.Query();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(c =>
                    c.CustomerName.Contains(searchString) ||
                    c.ContactNumber.Contains(searchString));
            }

            // Project to DTO directly in EF query
            var dtoQuery = query.Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Address = c.Address,
                ContactNumber = c.ContactNumber,
                ZipCode = c.ZipCode
            });

            return await PaginatedList<CustomerDto>.CreateAsync(dtoQuery, pageIndex, pageSize);
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return null;

            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,
                Address = customer.Address,
                ContactNumber = customer.ContactNumber,
                ZipCode = customer.ZipCode
            };
        }

        public async Task AddCustomerAsync(CustomerDto customerDto)
        {
            var existingCustomer = await _customerRepository
                .GetCustomerByNameAndContactNumberAsync(customerDto.CustomerName, customerDto.ContactNumber);

            if (existingCustomer != null)
            {
                throw new InvalidOperationException("A customer with the same name and contact number already exists.");
            }

            var newCustomer = new Customer
            {
                CustomerName = customerDto.CustomerName,
                Address = customerDto.Address,
                ContactNumber = customerDto.ContactNumber,
                ZipCode = customerDto.ZipCode
            };

            await _customerRepository.AddAsync(newCustomer);
            await _customerRepository.SaveChangesAsync();
        }

        public async Task UpdateCustomerAsync(CustomerDto customerDto)
        {
            var customer = await _customerRepository.GetByIdAsync(customerDto.CustomerId);
            if (customer == null) throw new KeyNotFoundException("Customer not found.");

            customer.CustomerName = customerDto.CustomerName;
            customer.Address = customerDto.Address;
            customer.ContactNumber = customerDto.ContactNumber;
            customer.ZipCode = customerDto.ZipCode;

            await _customerRepository.UpdateAsync(customer);
            await _customerRepository.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(int id)
        {
            await _customerRepository.DeleteAsync(id);
            await _customerRepository.SaveChangesAsync();
        }
    }
}
