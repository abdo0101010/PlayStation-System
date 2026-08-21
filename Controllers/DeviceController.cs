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
    public class DeviceController : Controller
    {
        IGenericService<Device> _deviceService;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly UserManager<ApplicationUser> _userManager;
        public DeviceController(IGenericService<Device> deviceService, ICurrentTenantService currentTenantService, UserManager<ApplicationUser> userManager)
        {
            _deviceService = deviceService;
            _currentTenantService = currentTenantService;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var devices = await _deviceService.GetAllAsync();
            ViewBag.DevicesCount = devices.Count();
            ViewBag.DevicesActive = devices.Count(d => d.IsActive);
            ViewBag.DevicesInactive = devices.Count(d => !d.IsActive);

            return View(devices);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Device device)
        {
            // إزالة شرط الـ TenantId من فحص المدخلات لأنه يُحقن من السيستم
            ModelState.Remove(nameof(device.TenantId));

            if (ModelState.IsValid)
            {
                // إسناد الـ TenantId الحالي إذا كان فارغاً
                if (string.IsNullOrEmpty(device.TenantId))
                {
                    device.TenantId = _currentTenantService.TenantId;

                    // في حال كان الـ Claim فارغاً، نجلبه من قاعدة البيانات مباشرة
                    if (string.IsNullOrEmpty(device.TenantId))
                    {
                        var user = await _userManager.GetUserAsync(User);
                        device.TenantId = user?.TenantId ?? string.Empty;
                    }
                }

                await _deviceService.AddAsync(device);
                TempData["SuccessMessage"] = "تم إضافة الجهاز بنجاح.";
                return RedirectToAction(nameof(Index));
            }

            return View(device);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var device = await _deviceService.GetByIdAsync(id);
            if (device == null)
            {
                return NotFound();
            }
            return View(device);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Device device)
        {
            if (ModelState.IsValid)
            {
              await  _deviceService.Update(device);
                return RedirectToAction("Index");
            }
            return View(device);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var device = await _deviceService.GetByIdAsync(id);
            if (device == null)
            {
                return NotFound();
            }
            await _deviceService.Delete(device);
           
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> GetPricing()
        {
            var devicesList = await _deviceService.GetAllAsync();

            var pricingList = devicesList.Select(device => new PricingDeviceViewMOdel
            {
                DeviceId = device.Id,
                DeviceName = device.Name,
                HourPriceSingle = device.HourPriceSingle,
                HourPriceMulti = device.HourPriceMulti,
                Type = device.Type,
                IsOccupied = device.IsActive
            }).ToList();

            ViewBag.AverageSinglePrice = pricingList.Any() ? pricingList.Average(p => p.HourPriceSingle) : 0;
            ViewBag.AverageMultiPrice = pricingList.Any() ? pricingList.Average(p => p.HourPriceMulti) : 0;

            // تم حذف return NotFound() حتى تُفتح الصفحة وتعرض جدولاً فارغاً إذا لم توجد أجهزة
            return View(pricingList);
        }
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var device = await _deviceService.GetByIdAsync(id);
            if (device == null) return NotFound();

            // عكس الحالة
            device.IsActive = !device.IsActive;
            await _deviceService.Update(device);

            TempData["SuccessMessage"] = $"تم تغيير حالة {device.Name} إلى {(device.IsActive ? "متاح" : "مشغول/معطل")} بنجاح.";
            return RedirectToAction(nameof(Index));
        }
    }
}
