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

            var result = await _returnService.GetCompletedOrdersAsync(searchString, fromDateParsed, toDateParsed, pageIndex, pageSize);

            ViewData["CurrentFilter"] = searchString;
            ViewData["FromDate"] = fromDate;
            ViewData["ToDate"] = toDate;

            if (result.IsSuccess)
                return View(result.Value);

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            return View(new PaginatedList<CompletedOrderDto>(new List<CompletedOrderDto>(), 0, pageIndex, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReturn(ReturnRequestDto returnRequest)
        {
            var result = await _returnService.ProcessReturnAsync(returnRequest);

            if (result.IsSuccess)
            {
                return Json(new { success = true, message = "Return processed successfully" });
            }

            return Json(new { success = false, message = string.Join("; ", result.Errors.Select(e => e.Message)) });
        }

        public async Task<IActionResult> History(string searchString, string fromDate, string toDate, int pageIndex = 1)
        {
            int pageSize = 10;

            DateOnly? fromDateParsed = null;
            DateOnly? toDateParsed = null;

            if (DateOnly.TryParse(fromDate, out var parsedFromDate))
                fromDateParsed = parsedFromDate;

            if (DateOnly.TryParse(toDate, out var parsedToDate))
                toDateParsed = parsedToDate;

            var result = await _returnService.GetReturnsAsync(searchString, fromDateParsed, toDateParsed, pageIndex, pageSize);

            ViewData["CurrentFilter"] = searchString;
            ViewData["FromDate"] = fromDate;
            ViewData["ToDate"] = toDate;

            if (result.IsSuccess)
                return View(result.Value);

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            return View(new PaginatedList<ReturnLogDto>(new List<ReturnLogDto>(), 0, pageIndex, pageSize));
        }
    }
}
