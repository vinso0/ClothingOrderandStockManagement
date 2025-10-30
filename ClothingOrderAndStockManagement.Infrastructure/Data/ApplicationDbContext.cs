using ClothingOrderAndStockManagement.Domain.Entities.Customers;
using ClothingOrderAndStockManagement.Domain.Entities.Products;
using ClothingOrderAndStockManagement.Domain.Entities.Orders;
using ClothingOrderAndStockManagement.Domain.Entities.Account;
using ClothingOrderAndStockManagement.Domain.EntityMappings; // Add this
using ClothingOrderAndStockManagement.Application.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<Users>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<OrderRecord> OrderRecords => Set<OrderRecord>();
        public DbSet<OrderPackage> OrderPackages => Set<OrderPackage>();
        public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();
        public DbSet<CustomerInfo> CustomerInfos => Set<CustomerInfo>();
        public DbSet<Package> Packages => Set<Package>();
        public DbSet<PackageItem> PackageItems => Set<PackageItem>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();
        public DbSet<InventoryLog> InventoryLogs => Set<InventoryLog>();
        public DbSet<ReturnLog> ReturnLogs => Set<ReturnLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerConfiguration).Assembly);
        }
    }
}
