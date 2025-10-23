using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    [Authorize(Roles = "Orders Admin, Owner")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IPackageService _packageService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            ICustomerService customerService,
            IPackageService packageService,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _customerService = customerService;
            _packageService = packageService;
            _logger = logger;
        }

        // List all orders
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllAsync();
            return View(orders);
        }

        // Create order for specific customer (GET)
        [HttpGet]
        public async Task<IActionResult> Create(int customerId)
        {
            var customerResult = await _customerService.GetCustomerByIdAsync(customerId);
            if (!customerResult.IsSuccess)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction("Index", "Customers");
            }

            var packages = await _packageService.GetAllPackagesAsync();

            ViewBag.Customer = customerResult.Value;
            ViewBag.Packages = packages.ToList();

            var model = new CreateOrderDto
            {
                CustomerId = customerId,
                UserId = User.Identity?.Name
            };

            return View(model);
        }

        // Create order WITHOUT payment (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewDataAsync(dto.CustomerId);
                return View(dto);
            }

            try
            {
                dto.UserId = User.Identity?.Name ?? "System";
                var orderId = await _orderService.CreateAsync(dto);
                TempData["Success"] = $"Order #{orderId} created successfully! You can now add payment.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                TempData["Error"] = $"Error creating order: {ex.Message}";
                await PopulateViewDataAsync(dto.CustomerId);
                return View(dto);
            }
        }

        // Add payment to existing order (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(AddPaymentDto dto, IFormFile? ProofImage, IFormFile? ProofImage2)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid payment data.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var success = await _orderService.AddPaymentAsync(dto, ProofImage, ProofImage2);

                if (success)
                {
                    TempData["Success"] = "Payment added successfully!";
                }
                else
                {
                    TempData["Error"] = "Order not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding payment");
                TempData["Error"] = $"Error adding payment: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Update order status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int OrderRecordsId, string OrderStatus)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(OrderRecordsId);
                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                order.OrderStatus = OrderStatus;
                await _orderService.UpdateAsync(order);
                TempData["Success"] = "Order status updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status");
                TempData["Error"] = $"Error updating status: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Delete order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _orderService.DeleteAsync(id);
                if (result)
                {
                    TempData["Success"] = "Order deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Order not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order");
                TempData["Error"] = $"Error deleting order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateViewDataAsync(int customerId)
        {
            var customerResult = await _customerService.GetCustomerByIdAsync(customerId);
            var packages = await _packageService.GetAllPackagesAsync();
            ViewBag.Customer = customerResult.Value;
            ViewBag.Packages = packages.ToList();
        }
    }
}