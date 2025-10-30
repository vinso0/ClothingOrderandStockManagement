using ClothingOrderAndStockManagement.Domain.Interfaces;
using ClothingOrderAndStockManagement.Application.Services;
using ClothingOrderAndStockManagement.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ClothingOrderAndStockManagement.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDI(this IServiceCollection services)
        {
            // application-level services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped<IAccountService, AccountService>();

            return services;
        }
    }
}
