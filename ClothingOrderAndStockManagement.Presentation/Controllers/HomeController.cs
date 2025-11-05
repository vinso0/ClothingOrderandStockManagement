using System.Diagnostics;
using ClothingOrderAndStockManagement.Domain.Entities;
using ClothingOrderAndStockManagement.Domain.Entities.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<Users> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<Users> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("User is null. Unable to retrieve roles.");
                return RedirectToAction("Error");
            }

            var roles = await _userManager.GetRolesAsync(user);

            ViewData["UserRoles"] = roles;


            return View();
        }

        [Authorize(Roles = "Owner")]
        public IActionResult Reports()
        {
            return RedirectToAction("Index", "Reports");
        }

        [Authorize(Roles = "Inventory Admin")]
        public IActionResult InventoryAdmin()
        {
            return RedirectToAction("Index", "Items");
        }

        [Authorize(Roles = "Orders Admin")]
        public IActionResult OrdersAdmin()
        {
            return RedirectToAction("Index", "Customers");
        }

        [Authorize(Roles = "Returns Admin")]
        public IActionResult ReturnsAdmin()
        {
            return RedirectToAction("Index", "Returns");
        }

        [Authorize(Roles = "Staff")]
        public IActionResult Staff()
        {
            return RedirectToAction("Index", "Staff");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
