using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Application.Repositories;
using ClothingOrderAndStockManagement.Application.Services;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using ClothingOrderAndStockManagement.Domain.Interfaces.Repositories;
using ClothingOrderAndStockManagement.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ClothingOrderAndStockManagement.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDI(this IServiceCollection services)
        {
            // Application services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped<IAccountService, AccountService>(); // Move AccountService to Application if needed

            // Application repositories
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            // Add other repositories as needed

            return services;
        }
    }
}
