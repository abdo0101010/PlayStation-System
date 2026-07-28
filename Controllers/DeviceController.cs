using Microsoft.AspNetCore.Mvc;
using PlaystationSystem.Models;
using PlaystationSystem.Services;

namespace PlaystationSystem.Controllers
{
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
            ViewBag.DevicesActive = devices.Count(d => !d.IsOccupied);
            ViewBag.DevicesInactive = devices.Count(d => d.IsOccupied);

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
                await _deviceService.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(device);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
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
                _deviceService.Update(device);
                await _deviceService.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(device);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var device = await _deviceService.GetByIdAsync(id);
            if (device == null)
            {
                return NotFound();
            }
            await _deviceService.Delete(device);
            await _deviceService.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
