using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Web.ViewModels.Account;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var (Success, User) = await _accountService.PasswordSignInAsync(
                model.UsernameOrEmail, model.Password, model.RememberMe);

            if (Success && User is not null)
            {
                var roles = await _accountService.GetRolesAsync(User!);

                // Redirect based on role (fallback to Home/Index)
                if (roles.Contains("Owner")) return RedirectToAction("Index", "Home");
                if (roles.Contains("Inventory Admin")) return RedirectToAction("Index", "Home");
                if (roles.Contains("Orders Admin")) return RedirectToAction("Index", "Home");
                if (roles.Contains("Returns Admin")) return RedirectToAction("Index", "Home");
                if (roles.Contains("Staff")) return RedirectToAction("Index", "Home");

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid Login Attempt.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
