using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    public class PackagesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
