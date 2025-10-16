using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // 🔹 List all orders
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllAsync();
            return View(orders);
        }

        // 🔹 View details of an order
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // 🔹 Create (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 🔹 Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            // Map to OrderRecordDto (since the service expects it)
            var newOrder = new OrderRecordDto
            {
                CustomerId = dto.CustomerId,
                OrderDatetime = dto.OrderDatetime,
                OrderStatus = dto.OrderStatus,
                UserId = dto.UserId
            };

            await _orderService.CreateAsync(newOrder);
            return RedirectToAction(nameof(Index));
        }

        // 🔹 Edit (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();

            var dto = new UpdateOrderDto
            {
                OrderRecordsId = order.OrderRecordsId,
                CustomerId = order.CustomerId,
                OrderDatetime = order.OrderDatetime,
                OrderStatus = order.OrderStatus,
                UserId = order.UserId
            };

            return View(dto);
        }

        // 🔹 Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateOrderDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            // Convert to OrderRecordDto for the service
            var updatedOrder = new OrderRecordDto
            {
                OrderRecordsId = dto.OrderRecordsId,
                CustomerId = dto.CustomerId,
                OrderDatetime = dto.OrderDatetime,
                OrderStatus = dto.OrderStatus,
                UserId = dto.UserId
            };

            await _orderService.UpdateAsync(updatedOrder);
            return RedirectToAction(nameof(Index));
        }

        // 🔹 Delete (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // 🔹 Delete (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _orderService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
