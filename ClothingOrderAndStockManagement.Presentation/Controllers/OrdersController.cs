using ClothingOrderAndStockManagement.Application.Dtos.Orders;
using ClothingOrderAndStockManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingOrderAndStockManagement.Web.Controllers
{
    [Authorize(Roles = "Orders Admin, Owner")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IPackageService _packageService;
        private readonly IWebHostEnvironment _environment;

        public OrdersController(
            IOrderService orderService,
            ICustomerService customerService,
            IPackageService packageService,
            IWebHostEnvironment environment)
        {
            _orderService = orderService;
            _customerService = customerService;
            _packageService = packageService;
            _environment = environment;
        }

        // List all orders
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllAsync();
            return View(orders);
        }

        // Create order for specific customer (GET)
        [HttpGet]
        public async Task<IActionResult> Create(int customerId)
        {
            var customerResult = await _customerService.GetCustomerByIdAsync(customerId);
            if (!customerResult.IsSuccess)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction("Index", "Customers");
            }

            var packages = await _packageService.GetAllPackagesAsync();

            ViewBag.Customer = customerResult.Value;
            ViewBag.Packages = packages.ToList();

            var model = new CreateOrderDto
            {
                CustomerId = customerId,
                UserId = User.Identity?.Name
            };

            return View(model);
        }

        // Create order (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderDto dto,
            IFormFile? ProofImage, IFormFile? ProofImage2)
        {
            if (!ModelState.IsValid)
            {
                var customerResult = await _customerService.GetCustomerByIdAsync(dto.CustomerId);
                var packages = await _packageService.GetAllPackagesAsync();
                ViewBag.Customer = customerResult.Value;
                ViewBag.Packages = packages.ToList();
                return View(dto);
            }

            try
            {
                // Handle payment proof uploads
                if (dto.InitialPayment != null && dto.InitialPayment.Amount > 0)
                {
                    if (ProofImage != null)
                    {
                        dto.InitialPayment.ProofUrl = await SavePaymentProof(ProofImage);
                    }
                    if (ProofImage2 != null)
                    {
                        dto.InitialPayment.ProofUrl2 = await SavePaymentProof(ProofImage2);
                    }
                }

                // Map to OrderRecordDto
                var orderDto = new OrderRecordDto
                {
                    CustomerId = dto.CustomerId,
                    OrderDatetime = dto.OrderDatetime,
                    OrderStatus = dto.OrderStatus,
                    UserId = dto.UserId ?? User.Identity?.Name ?? "System",
                    OrderPackages = dto.OrderPackages.Select(op => new OrderPackageDto
                    {
                        PackagesId = op.PackagesId,
                        Quantity = op.Quantity,
                        PriceAtPurchase = op.PriceAtPurchase
                    }).ToList(),
                    PaymentRecords = dto.InitialPayment != null && dto.InitialPayment.Amount > 0
                        ? new List<PaymentRecordDto>
                        {
                            new PaymentRecordDto
                            {
                                Amount = dto.InitialPayment.Amount,
                                ProofUrl = dto.InitialPayment.ProofUrl,
                                ProofUrl2 = dto.InitialPayment.ProofUrl2,
                                PaymentStatus = dto.InitialPayment.PaymentStatus,
                                PaymentDate = DateTime.Now
                            }
                        }
                        : new List<PaymentRecordDto>()
                };

                await _orderService.CreateAsync(orderDto);
                TempData["Success"] = "Order created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating order: {ex.Message}";
                var customerResult = await _customerService.GetCustomerByIdAsync(dto.CustomerId);
                var packages = await _packageService.GetAllPackagesAsync();
                ViewBag.Customer = customerResult.Value;
                ViewBag.Packages = packages.ToList();
                return View(dto);
            }
        }

        // Add payment to existing order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(AddPaymentDto dto,
            IFormFile? ProofImage, IFormFile? ProofImage2)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid payment data.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var order = await _orderService.GetByIdAsync(dto.OrderRecordsId);
                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Handle payment proof uploads
                string? proofUrl = null;
                string? proofUrl2 = null;

                if (ProofImage != null)
                {
                    proofUrl = await SavePaymentProof(ProofImage);
                }
                if (ProofImage2 != null)
                {
                    proofUrl2 = await SavePaymentProof(ProofImage2);
                }

                // Add new payment record
                var newPayment = new PaymentRecordDto
                {
                    Amount = dto.Amount,
                    ProofUrl = proofUrl,
                    ProofUrl2 = proofUrl2,
                    PaymentStatus = dto.PaymentStatus,
                    PaymentDate = DateTime.Now
                };

                order.PaymentRecords.Add(newPayment);

                // Update order status based on payment
                var totalPaid = order.PaymentRecords.Sum(p => p.Amount);
                if (totalPaid >= order.TotalAmount)
                {
                    order.OrderStatus = "Paid";
                }
                else if (totalPaid > 0)
                {
                    order.OrderStatus = "Partially Paid";
                }

                await _orderService.UpdateAsync(order);
                TempData["Success"] = "Payment added successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error adding payment: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Update order status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int OrderRecordsId, string OrderStatus)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(OrderRecordsId);
                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                order.OrderStatus = OrderStatus;
                await _orderService.UpdateAsync(order);
                TempData["Success"] = "Order status updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating status: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Delete order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _orderService.DeleteAsync(id);
                if (result)
                {
                    TempData["Success"] = "Order deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Order not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method to save payment proof
        private async Task<string> SavePaymentProof(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "payment-proofs");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/payment-proofs/{uniqueFileName}";
        }
    }
}