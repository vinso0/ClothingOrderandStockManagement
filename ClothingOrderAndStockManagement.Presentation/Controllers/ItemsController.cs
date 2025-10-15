using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    public class ItemsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
