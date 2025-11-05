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
            if (user == null) { _logger.LogWarning("User is null."); return RedirectToAction("Error"); }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Owner"))
                return RedirectToAction("Index", "Reports");      // Reports landing [ClothingOrderAndStockManagement.Presentation/Controllers/HomeController.cs]

            if (roles.Contains("Inventory Admin"))
                return RedirectToAction("Index", "Items");         // Items first in sidebar [ClothingOrderAndStockManagement.Presentation/Controllers/HomeController.cs]

            if (roles.Contains("Orders Admin"))
                return RedirectToAction("Index", "Customers");     // Customers first in sidebar [ClothingOrderAndStockManagement.Presentation/Controllers/HomeController.cs]

            if (roles.Contains("Returns Admin"))
                return RedirectToAction("Index", "Returns");       // Returns list [ClothingOrderAndStockManagement.Presentation/Controllers/HomeController.cs]

            if (roles.Contains("Staff"))
                return RedirectToAction("Index", "Staff");         // Staff landing [ClothingOrderAndStockManagement.Presentation/Controllers/HomeController.cs]

            return RedirectToAction("Error");                      // fallback [ClothingOrderAndStockManagement.Presentation/Controllers/HomeController.cs]
        }


    }
}
