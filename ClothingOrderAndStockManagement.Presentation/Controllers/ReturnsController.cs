using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    public class ReturnsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
