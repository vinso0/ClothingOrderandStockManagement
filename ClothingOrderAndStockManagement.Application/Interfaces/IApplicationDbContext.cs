using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        // Only methods, no DbSet properties
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();

        // Add a method to get DbSet instead of properties
        DbSet<T> Set<T>() where T : class;
    }
}
