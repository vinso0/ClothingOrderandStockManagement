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
        private readonly ILogger<OrdersController> _logger; // ADD THIS

        public OrdersController(
            IOrderService orderService,
            ICustomerService customerService,
            IPackageService packageService,
            IWebHostEnvironment environment,
            ILogger<OrdersController> logger) // ADD THIS
        {
            _orderService = orderService;
            _customerService = customerService;
            _packageService = packageService;
            _environment = environment;
            _logger = logger; // ADD THIS
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
        public async Task<IActionResult> Create(CreateOrderDto dto, IFormFile? ProofImage, IFormFile? ProofImage2)
        {
            _logger.LogInformation("=== ORDER CREATION STARTED ===");
            _logger.LogInformation($"Customer ID: {dto.CustomerId}");
            _logger.LogInformation($"ProofImage: {(ProofImage?.FileName ?? "None")}");
            _logger.LogInformation($"ProofImage2: {(ProofImage2?.FileName ?? "None")}");

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid");
                    foreach (var error in ModelState)
                    {
                        _logger.LogWarning($"ModelState Error - Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }

                    var customerResult = await _customerService.GetCustomerByIdAsync(dto.CustomerId);
                    var packages = await _packageService.GetAllPackagesAsync();
                    ViewBag.Customer = customerResult.Value;
                    ViewBag.Packages = packages.ToList();
                    return View(dto);
                }

                // Handle payment proof uploads
                if (dto.InitialPayment != null && dto.InitialPayment.Amount > 0)
                {
                    _logger.LogInformation($"Processing payment of {dto.InitialPayment.Amount}");

                    if (ProofImage != null)
                    {
                        _logger.LogInformation($"Processing ProofImage: {ProofImage.FileName}, Size: {ProofImage.Length}");
                        dto.InitialPayment.ProofUrl = await SavePaymentProof(ProofImage);
                        _logger.LogInformation($"ProofImage saved: {dto.InitialPayment.ProofUrl}");
                    }

                    if (ProofImage2 != null)
                    {
                        _logger.LogInformation($"Processing ProofImage2: {ProofImage2.FileName}, Size: {ProofImage2.Length}");
                        dto.InitialPayment.ProofUrl2 = await SavePaymentProof(ProofImage2);
                        _logger.LogInformation($"ProofImage2 saved: {dto.InitialPayment.ProofUrl2}");
                    }
                }

                // Map to OrderRecordDto
                _logger.LogInformation("Mapping to OrderRecordDto...");
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

                _logger.LogInformation("Calling _orderService.CreateAsync...");
                await _orderService.CreateAsync(orderDto);
                _logger.LogInformation("Order created successfully!");

                TempData["Success"] = "Order created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== ORDER CREATION FAILED ===");
                _logger.LogError($"Exception Type: {ex.GetType().Name}");
                _logger.LogError($"Message: {ex.Message}");
                _logger.LogError($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                }

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
        public async Task<IActionResult> AddPayment(AddPaymentDto dto, IFormFile? ProofImage, IFormFile? ProofImage2)
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
                if (ProofImage != null)
                {
                    dto.ProofUrl = await SavePaymentProof(ProofImage);
                }
                if (ProofImage2 != null)
                {
                    dto.ProofUrl2 = await SavePaymentProof(ProofImage2);
                }

                // Add new payment record
                var newPayment = new PaymentRecordDto
                {
                    Amount = dto.Amount,
                    ProofUrl = dto.ProofUrl,
                    ProofUrl2 = dto.ProofUrl2,
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

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException("Only image files (JPG, JPEG, PNG, GIF) are allowed.");
            }

            // Validate file size (10MB max)
            if (file.Length > 10 * 1024 * 1024)
            {
                throw new InvalidOperationException("File size must be less than 10MB.");
            }

            // Create uploads directory in a local folder (not wwwroot)
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "LocalStorage", "PaymentProofs");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var uniqueFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Return the local file path (this will be saved to database)
                return filePath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving file: {ex.Message}");
            }
        }
    }
}