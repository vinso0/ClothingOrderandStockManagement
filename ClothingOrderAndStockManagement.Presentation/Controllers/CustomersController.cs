using ClothingOrderAndStockManagement.Application.Dtos.Customers;
using ClothingOrderAndStockManagement.Application.Helpers;
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
            var result = await _customerService.GetCustomersAsync(searchString, pageIndex, pageSize);

            ViewData["CurrentFilter"] = searchString;

            if (result.IsSuccess)
                return View(result.Value);

            // Optionally, show an error view or message
            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            return View(new PaginatedList<CustomerDto>(new List<CustomerDto>(), 0, pageIndex, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerDto customerDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.AddCustomerAsync(customerDto);
                if (result.IsSuccess)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            }

            int pageIndex = 1;
            int pageSize = 5;
            var customersResult = await _customerService.GetCustomersAsync("", pageIndex, pageSize);

            ViewData["ShowAddCustomerModal"] = true;
            ViewData["AddCustomerModel"] = customerDto;

            return View("Index", customersResult.IsSuccess
                ? customersResult.Value
                : new PaginatedList<CustomerDto>(new List<CustomerDto>(), 0, pageIndex, pageSize));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _customerService.GetCustomerByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound();

            return View(result.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerDto customerDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.UpdateCustomerAsync(customerDto);
                if (result.IsSuccess)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            }

            return View(customerDto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}