using ClothingOrderAndStockManagement.Domain.Entities.Customers;
using ClothingOrderAndStockManagement.Domain.Entities.Account;


namespace ClothingOrderAndStockManagement.Domain.Entities.Orders;

public partial class ReturnLog
{
    public int ReturnLogsId { get; set; }

    public int OrderRecordsId { get; set; }

    // This property was added to match your SQL
    public int OrderPackagesId { get; set; }

    public int CustomerId { get; set; }

    public DateOnly ReturnDate { get; set; }

    public string? Reason { get; set; }


    // --- NAVIGATION PROPERTIES TO ADD ---
    public virtual OrderRecord OrderRecords { get; set; } = null!;

    // Add this for the relationship to OrderPackages
    public virtual OrderPackage OrderPackage { get; set; } = null!;

    // Add this for the relationship to CustomerInfo
    public virtual CustomerInfo CustomerInfo { get; set; } = null!;
}