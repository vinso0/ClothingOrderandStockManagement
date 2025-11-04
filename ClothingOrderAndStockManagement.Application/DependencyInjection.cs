using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Application.Services;
using ClothingOrderAndStockManagement.Domain.Interfaces;
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
            services.AddScoped<IReturnService, ReturnService>();

			return services;
        }
    }
}
