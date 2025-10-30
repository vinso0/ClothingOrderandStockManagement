using ClothingOrderAndStockManagement.Application.Dtos.Packages;
using ClothingOrderAndStockManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    public class PackagesController : Controller
    {
        private readonly IPackageService _packageService;
        private readonly IItemService _itemService;

        public PackagesController(IPackageService packageService, IItemService itemService)
        {
            _packageService = packageService;
            _itemService = itemService;
        }

        public async Task<IActionResult> Index(string searchString, int pageIndex = 1)
        {
            int pageSize = 5;
            var packagesResult = await _packageService.GetPackagesAsync(searchString, pageIndex, pageSize);

            if (packagesResult.IsFailed)
            {
                ModelState.AddModelError(string.Empty, "Error loading packages: " + packagesResult.Errors.FirstOrDefault()?.Message);
                return View();
            }

            ViewData["CurrentFilter"] = searchString;

            // Get items from ItemService - corrected approach
            var itemsResult = await _itemService.GetItemsAsync(1, 100, ""); // Get all items for selection
            ViewBag.Items = itemsResult.Where(i => i.Quantity > 0); // Filter available items

            return View(packagesResult.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePackageDto createPackageDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _packageService.AddPackageAsync(createPackageDto);
                    if (result.IsSuccess)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault()?.Message ?? "An error occurred while creating the package.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the package: " + ex.Message);
                }
            }

            int pageIndex = 1;
            int pageSize = 5;
            var packagesResult = await _packageService.GetPackagesAsync("", pageIndex, pageSize);

            ViewData["ShowAddPackageModal"] = true;
            ViewData["AddPackageModel"] = createPackageDto;

            // Get items from ItemService - corrected
            var itemsResult = await _itemService.GetItemsAsync(1, 100, "");
            ViewBag.Items = itemsResult.Where(i => i.Quantity > 0);

            return View("Index", packagesResult.IsSuccess ? packagesResult.Value : null);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var packageResult = await _packageService.GetPackageDetailsAsync(id);
            if (packageResult.IsFailed)
                return NotFound();

            var package = packageResult.Value;
            var updatePackageDto = new UpdatePackageDto
            {
                PackagesId = package.PackagesId,
                PackageName = package.PackageName,
                Description = package.Description,
                Price = package.Price,
                PackageItems = package.PackageItems.Select(pi => new UpdatePackageItemDto
                {
                    ItemId = pi.ItemId,
                    ItemQuantity = pi.ItemQuantity
                }).ToList()
            };

            // Get items from ItemService - corrected
            var itemsResult = await _itemService.GetItemsAsync(1, 100, "");
            ViewBag.Items = itemsResult.Where(i => i.Quantity > 0);

            return View(updatePackageDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdatePackageDto updatePackageDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _packageService.UpdatePackageAsync(updatePackageDto);
                    if (result.IsSuccess)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault()?.Message ?? "An error occurred while updating the package.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the package: " + ex.Message);
                }
            }

            // Get items from ItemService - corrected
            var itemsResult = await _itemService.GetItemsAsync(1, 100, "");
            ViewBag.Items = itemsResult.Where(i => i.Quantity > 0);

            return View(updatePackageDto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _packageService.DeletePackageAsync(id);
            if (result.IsFailed)
            {
                TempData["Error"] = result.Errors.FirstOrDefault()?.Message ?? "An error occurred while deleting the package.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var result = await _packageService.GetPackageDetailsAsync(id);
            if (result.IsFailed)
                return NotFound();

            return View(result.Value);
        }
    }
}
