namespace ClothingOrderAndStockManagement.Application.Dtos.Report
{
    public class SystemReportDto
    {
        // Overview Metrics
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalItems { get; set; }
        public int TotalPackages { get; set; }
        public int TotalUsers { get; set; }

        // Financial Metrics
        public decimal TotalRevenue { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal PendingPayments { get; set; }

        // Recent Activity (30 days)
        public int RecentOrdersCount { get; set; }
        public decimal RecentRevenue { get; set; }

        // Breakdowns
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public List<TopCustomerDto> TopCustomers { get; set; } = new();
        public List<PopularPackageDto> PopularPackages { get; set; } = new();
        public List<LowStockItemDto> LowStockItems { get; set; } = new();

        // Computed Properties
        public decimal CollectionRate => TotalRevenue > 0 ? (TotalPaid / TotalRevenue) * 100 : 0;
        public decimal AverageOrderValue => TotalOrders > 0 ? TotalRevenue / TotalOrders : 0;
    }

    public class TopCustomerDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int OrderCount { get; set; }
    }

    public class PopularPackageDto
    {
        public string PackageName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class LowStockItemDto
    {
        public string ItemName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
