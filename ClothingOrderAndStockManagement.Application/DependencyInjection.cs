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
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IItemCategoryService, ItemCategoryService>();

            // Application repositories
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
