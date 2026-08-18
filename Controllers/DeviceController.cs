using Microsoft.AspNetCore.Authorization;
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
        public DeviceController(IGenericService<Device> deviceService)
        {
            _deviceService = deviceService;
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
            if (ModelState.IsValid)
            {
                await _deviceService.AddAsync(device);
                return RedirectToAction("Index");
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
        public async Task<IActionResult> GetPricing()
        {
            var DevicesList = await _deviceService.GetAllAsync();
            var pricingList = new List<PricingDeviceViewMOdel>();
            foreach (var device in DevicesList)
            {
                var pricing = new PricingDeviceViewMOdel
                {
                    DeviceId = device.Id,
                    DeviceName = device.Name,
                    HourPriceSingle = device.HourPriceSingle,
                    HourPriceMulti = device.HourPriceMulti
                    ,Type = device.Type
                    ,IsOccupied = device.IsActive
                };
                pricingList.Add(pricing);
            }
            ViewBag.AverageSinglePrice = pricingList.Any() ? pricingList.Average(p => p.HourPriceSingle) : 0;
            ViewBag.AverageMultiPrice = pricingList.Any() ? pricingList.Average(p => p.HourPriceMulti) : 0;
            if (pricingList.Count == 0)
            {
                return NotFound();
            }
            return View(pricingList);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
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
