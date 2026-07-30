using Microsoft.AspNetCore.Mvc;
using PlaystationSystem.Models;
using PlaystationSystem.Services;
using PlaystationSystem.ViewModel;

namespace PlaystationSystem.Controllers
{
    public class DrinkController: Controller
    {
        IGenericService<Product> _productService;
        public DrinkController(IGenericService<Product> productService)
        {
            _productService = productService;
        }
        [HttpGet]
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
        public async Task<IActionResult> Create(DrinksInventoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = model.Name,
                    PurchasePrice = model.PurchasePrice,
                    SellingPrice = model.SellingPrice,
                    StockQuantity = model.StockQuantity
                };

                await _productService.AddAsync(product);
                await _productService.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService .GetByIdAsync(id);
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
        public async Task<IActionResult> Edit(int id, DrinksInventoryViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Id = model.Id,
                    Name = model.Name,
                    PurchasePrice = model.PurchasePrice,
                    SellingPrice = model.SellingPrice,
                    StockQuantity = model.StockQuantity
                };

                await _productService.Update(product);
                await _productService.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            await _productService.DeleteById(id);
            await _productService.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
