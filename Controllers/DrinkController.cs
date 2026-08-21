using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlaystationSystem.Models;
using PlaystationSystem.Services;
using PlaystationSystem.ViewModel;

namespace PlaystationSystem.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class DrinkController : Controller
    {
        private readonly IGenericService<Product> _productService;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DrinkController(
            IGenericService<Product> productService,
            ICurrentTenantService currentTenantService,
            UserManager<ApplicationUser> userManager)
        {
            _productService = productService;
            _currentTenantService = currentTenantService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync() ?? new List<Product>();

            var productViewModels = products.Select(p => new DrinksInventoryViewModel
            {
                Id = p.Id,
                Name = p.Name,
                PurchasePrice = p.PurchasePrice,
                SellingPrice = p.SellingPrice,
                StockQuantity = p.StockQuantity
            }).ToList();

            ViewBag.TotalProducts = productViewModels.Count;
            ViewBag.LowStockCount = productViewModels.Count(p => p.IsLowStock);
            ViewBag.TotalInventoryValue = productViewModels.Sum(p => p.StockQuantity * p.PurchasePrice);

            return View(productViewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DrinksInventoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                // الحصول على معرف المحل الحالي
                var tenantId = _currentTenantService.TenantId;
                if (string.IsNullOrEmpty(tenantId))
                {
                    var user = await _userManager.GetUserAsync(User);
                    tenantId = user?.TenantId ?? string.Empty;
                }

                var product = new Product
                {
                    Name = model.Name,
                    PurchasePrice = model.PurchasePrice,
                    SellingPrice = model.SellingPrice,
                    StockQuantity = model.StockQuantity,
                    TenantId = tenantId
                };

                await _productService.AddAsync(product);
                await _productService.SaveChangesAsync();

                TempData["SuccessMessage"] = "تمت إضافة المنتج إلى المخزن بنجاح.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            var model = new DrinksInventoryViewModel
            {
                Id = product.Id,
                Name = product.Name,
                PurchasePrice = product.PurchasePrice,
                SellingPrice = product.SellingPrice,
                StockQuantity = product.StockQuantity
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, DrinksInventoryViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingProduct = await _productService.GetByIdAsync(id);
                if (existingProduct == null) return NotFound();

                // تحديث الخصائص مع الاحتفاظ بـ TenantId الأصلي
                existingProduct.Name = model.Name;
                existingProduct.PurchasePrice = model.PurchasePrice;
                existingProduct.SellingPrice = model.SellingPrice;
                existingProduct.StockQuantity = model.StockQuantity;

                await _productService.Update(existingProduct);
                await _productService.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تحديث بيانات المنتج بنجاح.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            await _productService.Delete(product);
            await _productService.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف المنتج بنجاح.";
            return RedirectToAction(nameof(Index));
        }
    }
}