using ClothingOrderAndStockManagement.Application.Dtos.Items;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    public class ItemCategoriesController : Controller
    {
        private readonly IItemCategoryService _categoryService;

        public ItemCategoriesController(IItemCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string searchString, int pageIndex = 1)
        {
            const int pageSize = 5;
            var categories = await _categoryService.GetCategoriesAsync(pageIndex, pageSize, searchString ?? string.Empty);

            ViewData["CurrentFilter"] = searchString;
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateItemCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                // Reopen Add modal with the model
                ViewData["ShowAddCategoryModal"] = true;
                ViewData["AddCategoryModel"] = dto;

                // Reload first page with no filter to repopulate table
                var categories = await _categoryService.GetCategoriesAsync(1, 5, "");
                return View("Index", categories);
            }

            var result = await _categoryService.AddCategoryAsync(dto);
            if (result.IsSuccess)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));

            ViewData["ShowAddCategoryModal"] = true;
            ViewData["AddCategoryModel"] = dto;

            var reload = await _categoryService.GetCategoriesAsync(1, 5, "");
            return View("Index", reload);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateItemCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ShowEditCategoryModalId"] = dto.ItemCategoryId;
                ViewData["EditCategoryModel"] = dto;

                var categories = await _categoryService.GetCategoriesAsync(1, 5, "");
                return View("Index", categories);
            }

            var result = await _categoryService.UpdateCategoryAsync(dto);
            if (result.IsSuccess)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));

            ViewData["ShowEditCategoryModalId"] = dto.ItemCategoryId;
            ViewData["EditCategoryModel"] = dto;

            var reload = await _categoryService.GetCategoriesAsync(1, 5, "");
            return View("Index", reload);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result.IsSuccess)
                TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Message));

            return RedirectToAction(nameof(Index));
        }
    }
}
