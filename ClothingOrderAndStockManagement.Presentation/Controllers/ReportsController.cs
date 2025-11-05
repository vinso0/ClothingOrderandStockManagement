using ClothingOrderAndStockManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _reportService.GenerateSystemReportAsync(daysWindowForReturns: 90);

            if (result.IsSuccess)
                return View(result.Value);

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            return View(new ClothingOrderAndStockManagement.Application.Dtos.Report.SystemReportDto());
        }
    }
}
