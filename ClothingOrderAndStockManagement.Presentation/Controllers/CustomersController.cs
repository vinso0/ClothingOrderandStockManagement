using ClothingOrderAndStockManagement.Application.Dtos.Customers;
using ClothingOrderAndStockManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index(string searchString, int pageIndex = 1)
        {
            int pageSize = 5;

            // Directly get DTOs from service
            var customersPaginated = await _customerService.GetCustomersAsync(searchString, pageIndex, pageSize);

            ViewData["CurrentFilter"] = searchString;

            return View(customersPaginated);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerDto customerDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _customerService.AddCustomerAsync(customerDto);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            int pageIndex = 1;
            int pageSize = 5;

            // Fetch DTOs again to redisplay Index with validation errors
            var customersPaginated = await _customerService.GetCustomersAsync("", pageIndex, pageSize);

            // Preserve modal state
            ViewData["ShowAddCustomerModal"] = true;
            ViewData["AddCustomerModel"] = customerDto;

            return View("Index", customersPaginated);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customerDto = await _customerService.GetCustomerByIdAsync(id);
            if (customerDto == null) return NotFound();

            return View(customerDto); // Directly pass DTO to view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerDto customerDto)
        {
            if (ModelState.IsValid)
            {
                await _customerService.UpdateCustomerAsync(customerDto);
                return RedirectToAction(nameof(Index));
            }

            return View(customerDto); // Keep entered values if validation fails
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _customerService.DeleteCustomerAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
