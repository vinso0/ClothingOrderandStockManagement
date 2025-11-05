using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Dtos.Report;
using ClothingOrderAndStockManagement.Application.Dtos.Items;
using ClothingOrderAndStockManagement.Application.Dtos.Users;
using ClothingOrderAndStockManagement.Application.Dtos.Customers;
using ClothingOrderAndStockManagement.Application.Dtos.Packages;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class ReportsController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IItemService _itemService;
        private readonly IPackageService _packageService;
        private readonly IReturnService _returnService;
        private readonly IUserService _userService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IOrderService orderService,
            ICustomerService customerService,
            IItemService itemService,
            IPackageService packageService,
            IReturnService returnService,
            IUserService userService,
            ILogger<ReportsController> logger)
        {
            _orderService = orderService;
            _customerService = customerService;
            _itemService = itemService;
            _packageService = packageService;
            _returnService = returnService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var report = new SystemReportDto();

            // 1) Gather data using only existing interface methods (paginate to aggregate)
            var orders = await GetAllOrdersAsync(); // IOrderService exposes GetAllAsync used elsewhere in your app
            var customers = await GetAllCustomersAsync();
            var items = await GetAllItemsAsync();
            var packages = await _packageService.GetAllPackagesAsync(); // available "all" method
            var users = await GetAllUsersAsync();
            var returnsPaged = await GetReturnsWindowAsync(daysWindow: 90); // use existing returns paging API for a recent window

            // 2) Topline counts
            report.TotalOrders = orders.Count;
            report.TotalCustomers = customers.Count;
            report.TotalItems = items.Count;
            report.TotalPackages = packages.Count();
            report.TotalUsers = users.Count;

            // 3) Finance metrics (OrderRecordDto exposes computed totals)
            report.TotalRevenue = orders.Sum(o => o.TotalAmount);
            report.TotalPaid = orders.Sum(o => o.TotalPaid);
            report.PendingPayments = orders.Sum(o => o.RemainingBalance);

            // 4) Status breakdown
            report.OrdersByStatus = orders
                .GroupBy(o => string.IsNullOrWhiteSpace(o.OrderStatus) ? "Unknown" : o.OrderStatus)
                .ToDictionary(g => g.Key, g => g.Count());

            // 5) Recent activity (30 days)
            var since = DateTime.Now.AddDays(-30);
            var recent = orders.Where(o => o.OrderDatetime >= since).ToList();
            report.RecentOrdersCount = recent.Count;
            report.RecentRevenue = recent.Sum(o => o.TotalAmount);

            // 6) Top customers
            report.TopCustomers = orders
                .GroupBy(o => new { o.CustomerId, o.CustomerName })
                .Select(g => new TopCustomerDto
                {
                    CustomerName = g.Key.CustomerName,
                    TotalSpent = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToList();

            // 7) Popular packages
            report.PopularPackages = orders
                .SelectMany(o => o.OrderPackages ?? Enumerable.Empty<OrderPackageDto>())
                .GroupBy(p => string.IsNullOrWhiteSpace(p.PackageName) ? "Unknown" : p.PackageName)
                .Select(g => new PopularPackageDto
                {
                    PackageName = g.Key,
                    TotalSold = g.Sum(p => p.Quantity),
                    Revenue = g.Sum(p => p.PriceAtPurchase * p.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList();

            // 8) Low-stock items (ItemDto should expose StockQuantity and CategoryName; if not, omit CategoryName)
            report.LowStockItems = items
                .Where(i => i.Quantity <= 10 && i.Quantity > 0)
                .OrderBy(i => i.Quantity)
                .Take(10)
                .Select(i => new LowStockItemDto
                {
                    ItemName = $"#{i.ItemId} {i.ItemCategoryType} {i.Color ?? ""} {i.Size ?? ""}".Trim(),
                    CurrentStock = i.Quantity,
                    CategoryName = i.ItemCategoryType
                })
                .ToList();


            return View(report);
        }

        // Helpers: page through interfaces that are paginated-only

        private async Task<List<OrderRecordDto>> GetAllOrdersAsync()
        {
            // Your app already uses _orderService.GetAllAsync in OrdersController Index; reuse it directly.
            var all = await _orderService.GetAllAsync();
            return (all?.ToList()) ?? new List<OrderRecordDto>();
        }

        private async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
            var page = 1;
            const int size = 50;
            var acc = new List<CustomerDto>();

            while (true)
            {
                var res = await _customerService.GetCustomersAsync(searchString: "", pageIndex: page, pageSize: size);
                if (!res.IsSuccess || res.Value == null || !res.Value.Any()) break;

                acc.AddRange(res.Value);
                if (res.Value.Count < size) break; // no more pages
                page++;
            }
            return acc;
        }

        private async Task<List<ItemDto>> GetAllItemsAsync()
        {
            var page = 1;
            const int size = 50;
            var acc = new List<ItemDto>();

            while (true)
            {
                var paged = await _itemService.GetItemsAsync(pageNumber: page, pageSize: size, searchTerm: null);
                if (paged == null || !paged.Any()) break;

                acc.AddRange(paged);
                if (paged.Count < size) break;
                page++;
            }
            return acc;
        }

        private async Task<List<UserDto>> GetAllUsersAsync()
        {
            var page = 1;
            const int size = 50;
            var acc = new List<UserDto>();

            while (true)
            {
                var res = await _userService.GetUsersAsync(searchString: "", pageIndex: page, pageSize: size);
                if (!res.IsSuccess || res.Value == null || !res.Value.Any()) break;

                acc.AddRange(res.Value);
                if (res.Value.Count < size) break;
                page++;
            }
            return acc;
        }

        private async Task<int> GetReturnsWindowAsync(int daysWindow)
        {
            var page = 1;
            const int size = 50;
            var count = 0;
            var from = DateOnly.FromDateTime(DateTime.Now.AddDays(-daysWindow));
            var to = DateOnly.FromDateTime(DateTime.Now);

            while (true)
            {
                var res = await _returnService.GetReturnsAsync(
                    searchString: "",
                    fromDate: from,
                    toDate: to,
                    pageIndex: page,
                    pageSize: size);

                if (!res.IsSuccess || res.Value == null || !res.Value.Any()) break;

                count += res.Value.Count;
                if (res.Value.Count < size) break;
                page++;
            }
            return count;
        }
    }
}
