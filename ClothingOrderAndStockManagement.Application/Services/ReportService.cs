using ClothingOrderAndStockManagement.Application.Dtos.Customers;
using ClothingOrderAndStockManagement.Application.Dtos.Items;
using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Dtos.Packages;
using ClothingOrderAndStockManagement.Application.Dtos.Report;
using ClothingOrderAndStockManagement.Application.Dtos.Users;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using FluentResults;

namespace ClothingOrderAndStockManagement.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IItemService _itemService;
        private readonly IPackageService _packageService;
        private readonly IReturnService _returnService;
        private readonly IUserService _userService;

        public ReportService(
            IOrderService orderService,
            ICustomerService customerService,
            IItemService itemService,
            IPackageService packageService,
            IReturnService returnService,
            IUserService userService)
        {
            _orderService = orderService;
            _customerService = customerService;
            _itemService = itemService;
            _packageService = packageService;
            _returnService = returnService;
            _userService = userService;
        }

        public async Task<Result<SystemReportDto>> GenerateSystemReportAsync(int daysWindowForReturns)
        {
            try
            {
                var ordersRes = await _orderService.GetAllAsync();
                if (ordersRes.IsFailed) return Result.Fail<SystemReportDto>(ordersRes.Errors);
                var orders = ordersRes.Value?.ToList() ?? new List<OrderRecordDto>();
                var customers = await GetAllCustomersAsync();
                var items = await GetAllItemsAsync();
                var packages = await _packageService.GetAllPackagesAsync();
                var users = await GetAllUsersAsync();
                var returnsCount = await GetReturnsWindowAsync(daysWindowForReturns);

                var report = new SystemReportDto
                {
                    TotalOrders = orders.Count,
                    TotalCustomers = customers.Count,
                    TotalItems = items.Count,
                    TotalPackages = packages.Count(),
                    TotalUsers = users.Count,

                    TotalRevenue = orders.Sum(o => o.TotalAmount),
                    TotalPaid = orders.Sum(o => o.TotalPaid),
                    PendingPayments = orders.Sum(o => o.RemainingBalance),

                    OrdersByStatus = orders
                        .GroupBy(o => string.IsNullOrWhiteSpace(o.OrderStatus) ? "Unknown" : o.OrderStatus)
                        .ToDictionary(g => g.Key, g => g.Count()),

                    RecentOrdersCount = orders.Count(o => o.OrderDatetime >= DateTime.Now.AddDays(-30)),
                    RecentRevenue = orders.Where(o => o.OrderDatetime >= DateTime.Now.AddDays(-30))
                                          .Sum(o => o.TotalAmount),

                    TopCustomers = orders
                        .GroupBy(o => new { o.CustomerId, o.CustomerName })
                        .Select(g => new TopCustomerDto
                        {
                            CustomerName = g.Key.CustomerName,
                            TotalSpent = g.Sum(o => o.TotalAmount),
                            OrderCount = g.Count()
                        })
                        .OrderByDescending(x => x.TotalSpent)
                        .Take(5)
                        .ToList(),

                    PopularPackages = orders
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
                        .ToList(),

                    LowStockItems = items
                        .Where(i => i.Quantity <= 10 && i.Quantity > 0)
                        .OrderBy(i => i.Quantity)
                        .Take(10)
                        .Select(i => new LowStockItemDto
                        {
                            ItemName = $"#{i.ItemId} {i.ItemCategoryType} {i.Color ?? ""} {i.Size ?? ""}".Trim(),
                            CurrentStock = i.Quantity,
                            CategoryName = i.ItemCategoryType
                        })
                        .ToList()
                };

                return Result.Ok(report);
            }
            catch (Exception ex)
            {
                return Result.Fail<SystemReportDto>(ex.Message);
            }
        }


        private async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
            var page = 1;
            const int size = 50;
            var acc = new List<CustomerDto>();

            while (true)
            {
                var res = await _customerService.GetCustomersAsync(searchString: "", pageIndex: page, pageSize: size);
                if (res.IsFailed || res.Value == null || !res.Value.Any()) break;

                acc.AddRange(res.Value);
                if (res.Value.Count < size) break;
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
                if (res.IsFailed || res.Value == null || !res.Value.Any()) break;

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

                if (res.IsFailed || res.Value == null || !res.Value.Any()) break;

                count += res.Value.Count;
                if (res.Value.Count < size) break;
                page++;
            }
            return count;
        }
    }
}
