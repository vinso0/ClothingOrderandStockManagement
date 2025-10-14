using ClothingOrderAndStockManagement.Application.Dtos.Users;
using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(IUserService userService, UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userService = userService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchString, int pageIndex = 1)
        {
            int pageSize = 5;
            var usersPaginated = await _userService.GetUsersAsync(searchString, pageIndex, pageSize);

            ViewData["CurrentFilter"] = searchString;

            var roles = await _roleManager.Roles
                .Where(r => r.Name != "Owner")
                .Select(r => r.Name)
                .ToListAsync();

            ViewBag.Roles = new SelectList(roles);

            return View(usersPaginated);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserDto model)
        {
            if (ModelState.IsValid)
            {
                await _userService.CreateUserAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return BadRequest(ModelState);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var currentRole = userRoles.FirstOrDefault();

            var roles = await _roleManager.Roles
                .Where(r => r.Name != "Owner")
                .Select(r => r.Name)
                .ToListAsync();

            var model = new EditUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = currentRole
            };

            ViewBag.Roles = new SelectList(roles, model.Role);

            return PartialView("_EditUserModal", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserDto model)
        {
            if (ModelState.IsValid)
            {
                await _userService.UpdateUserAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _userService.DeleteUserAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
