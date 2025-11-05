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

        public StaffController(IOrderService orderService, ILogger<StaffController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // Shows only Partially Paid and Fully Paid orders
        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            const int pageSize = 5;
            var result = await _orderService.GetStaffOrdersAsync(pageIndex, pageSize);

            if (result.IsSuccess)
                return View(result.Value);

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            return View(new PaginatedList<OrderRecordDto>(new List<OrderRecordDto>(), 0, pageIndex, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            var result = await _orderService.CompleteOrderAsync(orderId);

            if (result.IsSuccess)
            {
                TempData["Success"] = $"Order #{orderId} has been successfully completed and processed!";
                _logger.LogInformation("Order {OrderId} marked as completed by staff", orderId);
            }
            else
            {
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger.LogWarning("Failed to complete order {OrderId}: {Errors}", orderId, string.Join("; ", result.Errors.Select(e => e.Message)));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
