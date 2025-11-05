using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Domain.Interfaces;
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

        public async Task<IActionResult> Index(string? status, int pageIndex = 1)
        {
            var result = await _orderService.GetFilteredOrdersAsync(status, pageIndex);

            ViewData["CurrentStatus"] = string.IsNullOrWhiteSpace(status) ? "" : status.Trim();

            if (result.IsSuccess)
                return View(result.Value);

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            return View(new ClothingOrderAndStockManagement.Application.Helpers.PaginatedList<OrderRecordDto>(
                new List<OrderRecordDto>(), 0, pageIndex, 5));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var result = await _orderService.GetByIdAsync(id);

            if (!result.IsSuccess)
                return NotFound();

            return PartialView("Partials/_OrderDetailsModal", result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int customerId)
        {
            var customerResult = await _customerService.GetCustomerByIdAsync(customerId);
            if (!customerResult.IsSuccess)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction("Index", "Customers");
            }

            await PopulateViewDataAsync(customerId);

            var model = new CreateOrderDto
            {
                CustomerId = customerId,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewDataAsync(dto.CustomerId);
                return View(dto);
            }

            var result = await _orderService.CreateAsync(dto);

            if (result.IsSuccess)
            {
                TempData["Success"] = $"Order #{result.Value} created successfully! You can now add payment.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            await PopulateViewDataAsync(dto.CustomerId);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(AddPaymentDto dto, IFormFile? ProofImage, IFormFile? ProofImage2)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid payment data.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _orderService.AddPaymentAsync(dto, ProofImage, ProofImage2);

            if (result.IsSuccess)
            {
                TempData["Success"] = "Payment added successfully!";
            }
            else
            {
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Message));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int OrderRecordsId, string OrderStatus)
        {
            var result = await _orderService.UpdateOrderStatusAsync(OrderRecordsId, OrderStatus);

            if (result.IsSuccess)
            {
                TempData["Success"] = "Order status updated successfully!";
            }
            else
            {
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Message));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, "Cancelled");

            if (result.IsSuccess)
            {
                TempData["Success"] = "Order cancelled successfully!";
            }
            else
            {
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Message));
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
