using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    public class ReturnsController : Controller
    {
        private readonly IReturnService _returnService;

        public ReturnsController(IReturnService returnService)
        {
            _returnService = returnService;
        }

        public async Task<IActionResult> Index(string searchString, string fromDate, string toDate, int pageIndex = 1)
        {
            int pageSize = 10;

            DateOnly? fromDateParsed = null;
            DateOnly? toDateParsed = null;

            if (DateOnly.TryParse(fromDate, out var parsedFromDate))
                fromDateParsed = parsedFromDate;

            if (DateOnly.TryParse(toDate, out var parsedToDate))
                toDateParsed = parsedToDate;

            // Completed orders for processing returns
            var completed = await _returnService.GetCompletedOrdersAsync(searchString, fromDateParsed, toDateParsed, pageIndex, pageSize);

            // Return history (use same filters and page for simplicity; can split later)
            var history = await _returnService.GetReturnsAsync(searchString, fromDateParsed, toDateParsed, pageIndex, pageSize);

            ViewData["CurrentFilter"] = searchString;
            ViewData["FromDate"] = fromDate;
            ViewData["ToDate"] = toDate;

            var vm = new ReturnsIndexDto
            {
                Completed = completed.IsSuccess
                    ? completed.Value
                    : new PaginatedList<CompletedOrderDto>(new List<CompletedOrderDto>(), 0, pageIndex, pageSize),
                History = history.IsSuccess
                    ? history.Value
                    : new PaginatedList<ReturnLogDto>(new List<ReturnLogDto>(), 0, pageIndex, pageSize)
            };

            if (!completed.IsSuccess)
                ModelState.AddModelError(string.Empty, string.Join("; ", completed.Errors.Select(e => e.Message)));
            if (!history.IsSuccess)
                ModelState.AddModelError(string.Empty, string.Join("; ", history.Errors.Select(e => e.Message)));

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReturn(ReturnRequestDto returnRequest)
        {
            var result = await _returnService.ProcessReturnAsync(returnRequest);
            if (result.IsSuccess)
                return Json(new { success = true, message = "Return processed successfully" });

            return Json(new { success = false, message = string.Join("; ", result.Errors.Select(e => e.Message)) });
        }
    }
}
