using ClothingOrderAndStockManagement.Application.Dtos.Items;
using ClothingOrderAndStockManagement.Application.Helpers;
using ClothingOrderAndStockManagement.Application.Interfaces;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    public class ItemsController : Controller
    {
        private readonly IItemService _itemService;

        public ItemsController(IItemService itemService)
        {
            _itemService = itemService;
        }

        public async Task<IActionResult> Index(string searchString, int pageIndex = 1)
        {
            const int pageSize = 5;

            var items = await _itemService.GetItemsAsync(pageIndex, pageSize, searchString);

            ViewData["CurrentFilter"] = searchString;
            ViewBag.Categories = await _itemService.GetItemCategoriesAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateItemDto createItemDto)
        {
            if (!ModelState.IsValid)
            {
                return await RebuildIndexForCreateAsync(createItemDto);
            }

            var result = await _itemService.CreateItemAsync(createItemDto);
            if (result.IsSuccess)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            return await RebuildIndexForCreateAsync(createItemDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _itemService.GetItemByIdAsync(id);
            if (item == null)
                return NotFound();

            var updateItemDto = new UpdateItemDto
            {
                ItemId = item.ItemId,
                ItemCategoryId = item.ItemCategoryId,
                Size = item.Size,
                Color = item.Color,
                Quantity = item.Quantity
            };

            ViewBag.Categories = await _itemService.GetItemCategoriesAsync();
            return View(updateItemDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateItemDto updateItemDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _itemService.GetItemCategoriesAsync();
                return View(updateItemDto);
            }

            var result = await _itemService.UpdateItemAsync(updateItemDto);
            if (result.IsSuccess)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            ViewBag.Categories = await _itemService.GetItemCategoriesAsync();
            return View(updateItemDto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _itemService.DeleteItemAsync(id);
            if (!result.IsSuccess)
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Message));
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> RebuildIndexForCreateAsync(CreateItemDto createItemDto)
        {
            const int pageIndex = 1;
            const int pageSize = 5;

            var items = await _itemService.GetItemsAsync(pageIndex, pageSize, "");
            ViewData["ShowAddItemModal"] = true;
            ViewData["AddItemModel"] = createItemDto;
            ViewBag.Categories = await _itemService.GetItemCategoriesAsync();

            return View("Index", items);
        }
    }
}
