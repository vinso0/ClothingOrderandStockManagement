using ClothingOrderAndStockManagement.Application.Dtos.Customers;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Customers;
using FluentResults;

namespace ClothingOrderAndStockManagement.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Result<PaginatedList<CustomerDto>>> GetCustomersAsync(string searchString, int pageIndex, int pageSize)
        {
            try
            {
                var query = _customerRepository.Query();

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    query = query.Where(c =>
                        c.CustomerName.Contains(searchString) ||
                        c.ContactNumber.Contains(searchString));
                }

                var dtoQuery = query.Select(c => new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    CustomerName = c.CustomerName,
                    Address = c.Address,
                    ContactNumber = c.ContactNumber,
                    ZipCode = c.ZipCode
                });

                var paginatedList = await PaginatedList<CustomerDto>.CreateAsync(dtoQuery, pageIndex, pageSize);
                return Result.Ok(paginatedList);
            }
            catch (Exception ex)
            {
                return Result.Fail<PaginatedList<CustomerDto>>(ex.Message);
            }
        }

        public async Task<Result<CustomerDto>> GetCustomerByIdAsync(int id)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(id);
                if (customer == null)
                    return Result.Fail<CustomerDto>("CustomerInfo not found.");

                var dto = new CustomerDto
                {
                    CustomerId = customer.CustomerId,
                    CustomerName = customer.CustomerName,
                    Address = customer.Address,
                    ContactNumber = customer.ContactNumber,
                    ZipCode = customer.ZipCode
                };

                return Result.Ok(dto);
            }
            catch (Exception ex)
            {
                return Result.Fail<CustomerDto>(ex.Message);
            }
        }

        public async Task<Result> AddCustomerAsync(CustomerDto customerDto)
        {
            try
            {
                var existingCustomer = await _customerRepository
                    .GetCustomerByNameAndContactNumberAsync(customerDto.CustomerName, customerDto.ContactNumber);

                if (existingCustomer != null)
                {
                    return Result.Fail("A customer with the same name and contact number already exists.");
                }

                var newCustomer = new CustomerInfo
                {
                    CustomerName = customerDto.CustomerName,
                    Address = customerDto.Address,
                    ContactNumber = customerDto.ContactNumber,
                    ZipCode = customerDto.ZipCode
                };

                await _customerRepository.AddAsync(newCustomer);
                await _customerRepository.SaveChangesAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Result> UpdateCustomerAsync(CustomerDto customerDto)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerDto.CustomerId);
                if (customer == null)
                    return Result.Fail("CustomerInfo not found.");

                customer.CustomerName = customerDto.CustomerName;
                customer.Address = customerDto.Address;
                customer.ContactNumber = customerDto.ContactNumber;
                customer.ZipCode = customerDto.ZipCode;

                await _customerRepository.UpdateAsync(customer);
                await _customerRepository.SaveChangesAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public async Task<Result> DeleteCustomerAsync(int id)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(id);
                if (customer == null)
                    return Result.Fail("CustomerInfo not found.");

                await _customerRepository.DeleteAsync(id);
                await _customerRepository.SaveChangesAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }
    }
}