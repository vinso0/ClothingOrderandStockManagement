using ClothingOrderAndStockManagement.Application.Dtos.Users;
using ClothingOrderAndStockManagement.Application.Helpers;
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
            var result = await _userService.GetUsersAsync(searchString, pageIndex, pageSize);

            ViewData["CurrentFilter"] = searchString;

            var roles = await _roleManager.Roles
                .Where(r => r.Name != "Owner")
                .Select(r => r.Name)
                .ToListAsync();

            ViewBag.Roles = new SelectList(roles);

            if (result.IsSuccess)
                return View(result.Value);

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            return View(new PaginatedList<UserDto>(new List<UserDto>(), 0, pageIndex, pageSize));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserDto model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.CreateUserAsync(model);
                if (result.IsSuccess)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            }
            return BadRequest(ModelState);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var result = await _userService.GetUserByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound();

            var roles = await _roleManager.Roles
                .Where(r => r.Name != "Owner")
                .Select(r => r.Name)
                .ToListAsync();

            ViewBag.Roles = new SelectList(roles, result.Value.Role);

            var model = new EditUserDto
            {
                Id = result.Value.Id,
                UserName = result.Value.UserName,
                Email = result.Value.Email,
                Role = result.Value.Role
            };

            return PartialView("_EditUserModal", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserDto model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.UpdateUserAsync(model);
                if (result.IsSuccess)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            // Optionally handle errors here
            return RedirectToAction(nameof(Index));
        }
    }
}