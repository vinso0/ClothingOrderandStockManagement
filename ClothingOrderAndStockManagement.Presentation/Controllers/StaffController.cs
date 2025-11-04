using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    [Authorize(Roles = "Staff, Owner")]
    public class StaffController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(
            IOrderService orderService,
            ILogger<StaffController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // Order sorting page - shows only Partially Paid and Fully Paid orders
        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            try
            {
                // Get filtered orders from service
                var pendingOrders = await _orderService.GetOrdersForSortingAsync();

                const int pageSize = 5;
                var totalCount = pendingOrders.Count();
                var pagedOrders = pendingOrders
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var paginatedList = new PaginatedList<OrderRecordDto>(pagedOrders, totalCount, pageIndex, pageSize);

                return View(paginatedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for sorting");
                TempData["Error"] = "Error loading orders. Please try again.";
                return View(new PaginatedList<OrderRecordDto>(new List<OrderRecordDto>(), 0, pageIndex, 5));
            }
        }

        // Complete order processing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(orderId);
                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Verify the order is in a valid state for completion
                var allowedStatuses = new[] { "Partially Paid", "Fully Paid" };
                if (!allowedStatuses.Contains(order.OrderStatus))
                {
                    TempData["Error"] = "Order cannot be completed. Only Partially Paid or Fully Paid orders can be marked as completed.";
                    return RedirectToAction(nameof(Index));
                }

                // Update status to Completed
                order.OrderStatus = "Completed";
                await _orderService.UpdateAsync(order);

                TempData["Success"] = $"Order #{order.OrderRecordsId} has been successfully completed and processed!";
                _logger.LogInformation("Order {OrderId} marked as completed by staff", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing order {OrderId}", orderId);
                TempData["Error"] = $"Error completing order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
