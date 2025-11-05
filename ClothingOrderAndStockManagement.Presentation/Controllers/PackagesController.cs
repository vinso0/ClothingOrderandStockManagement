using ClothingOrderAndStockManagement.Application.Dtos.Packages;
using ClothingOrderAndStockManagement.Application.Helpers;
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
            var result = await _packageService.GetPackagesAsync(searchString, pageIndex, pageSize);

            ViewData["CurrentFilter"] = searchString;

            var itemsResult = await _itemService.GetItemsAsync(1, 100, "");
            ViewBag.Items = itemsResult.Where(i => i.Quantity > 0);

            if (result.IsSuccess)
                return View(result.Value);

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            return View(new PaginatedList<PackageDto>(new List<PackageDto>(), 0, pageIndex, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePackageDto createPackageDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _packageService.AddPackageAsync(createPackageDto);
                if (result.IsSuccess)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            }

            int pageIndex = 1;
            int pageSize = 5;
            var packagesResult = await _packageService.GetPackagesAsync("", pageIndex, pageSize);

            ViewData["ShowAddPackageModal"] = true;
            ViewData["AddPackageModel"] = createPackageDto;

            var itemsResult = await _itemService.GetItemsAsync(1, 100, "");
            ViewBag.Items = itemsResult.Where(i => i.Quantity > 0);

            return View("Index", packagesResult.IsSuccess
                ? packagesResult.Value
                : new PaginatedList<PackageDto>(new List<PackageDto>(), 0, pageIndex, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdatePackageDto updatePackageDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _packageService.UpdatePackageAsync(updatePackageDto);
                if (result.IsSuccess)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Message)));
            }

            int pageIndex = 1;
            int pageSize = 5;
            var packagesResult = await _packageService.GetPackagesAsync("", pageIndex, pageSize);

            ViewData["ShowEditPackageModalId"] = updatePackageDto.PackagesId;
            ViewData["EditPackageModel"] = updatePackageDto;

            var itemsResult = await _itemService.GetItemsAsync(1, 100, "");
            ViewBag.Items = itemsResult.Where(i => i.Quantity > 0);

            return View("Index", packagesResult.IsSuccess
                ? packagesResult.Value
                : new PaginatedList<PackageDto>(new List<PackageDto>(), 0, pageIndex, pageSize));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _packageService.DeletePackageAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
