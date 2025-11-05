using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    public class ReturnsController : Controller
    {
        private readonly IReturnService _returnService;
        private readonly IOrderService _orderService;

        public ReturnsController(IReturnService returnService, IOrderService orderService)
        {
            _returnService = returnService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index(string searchString, string fromDate, string toDate,
                                              int completedPageIndex = 1, int historyPageIndex = 1)
        {
            int pageSize = 10;

            DateOnly? fromDateParsed = null;
            DateOnly? toDateParsed = null;

            if (DateOnly.TryParse(fromDate, out var parsedFromDate))
                fromDateParsed = parsedFromDate;

            if (DateOnly.TryParse(toDate, out var parsedToDate))
                toDateParsed = parsedToDate;

            var completedOrders = await _orderService.GetOrdersForReturnsAsync(searchString, fromDateParsed, toDateParsed, completedPageIndex, pageSize);

            var returnHistory = await _returnService.GetReturnsAsync(searchString, fromDateParsed, toDateParsed, historyPageIndex, pageSize);

            ViewData["CurrentFilter"] = searchString;
            ViewData["FromDate"] = fromDate;
            ViewData["ToDate"] = toDate;

            var vm = new ReturnsIndexViewModel
            {
                CompletedOrders = completedOrders.IsSuccess
                    ? completedOrders.Value
                    : new PaginatedList<OrderRecordDto>(new List<OrderRecordDto>(), 0, completedPageIndex, pageSize),
                ReturnHistory = returnHistory.IsSuccess
                    ? returnHistory.Value
                    : new PaginatedList<ReturnLogDto>(new List<ReturnLogDto>(), 0, historyPageIndex, pageSize)
            };

            if (!completedOrders.IsSuccess)
                ModelState.AddModelError(string.Empty, string.Join("; ", completedOrders.Errors.Select(e => e.Message)));
            if (!returnHistory.IsSuccess)
                ModelState.AddModelError(string.Empty, string.Join("; ", returnHistory.Errors.Select(e => e.Message)));

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReturn([FromBody] ReturnRequestDto returnRequest)
        {
            var result = await _returnService.ProcessReturnAsync(returnRequest);
            if (result.IsSuccess)
                return Json(new { success = true, message = "Return processed successfully" });

            return Json(new { success = false, message = string.Join("; ", result.Errors.Select(e => e.Message)) });
        }
    }
}
