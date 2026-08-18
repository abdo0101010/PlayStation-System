using Microsoft.AspNetCore.Mvc;
using PlaystationSystem.Models;
using PlaystationSystem.Services;
using PlaystationSystem.ViewModel;
using System.Security.Claims;

namespace PlaystationSystem.Controllers
{
    public class SessionController : Controller
    {
        private readonly IGenericService<Session> _sessionService;
        private readonly IGenericService<Customer> _customerService;
        private readonly IGenericService<Device> _deviceService;
        private readonly IShiftServices _shiftServices;
        private readonly IGenericService<Product> _productService;
        private readonly IGenericService<SessionOrder> _sessionOrderService;

        public SessionController(
            IGenericService<Session> sessionService,
            IGenericService<Customer> customerService,
            IGenericService<Device> deviceService,
            IShiftServices shiftServices,
            IGenericService<Product> productService,
            IGenericService<SessionOrder> sessionOrderService)
        {
            _sessionService = sessionService;
            _customerService = customerService;
            _deviceService = deviceService;
            _shiftServices = shiftServices;
            _productService = productService;
            _sessionOrderService = sessionOrderService;
        }

        // 1. فتح شاشة بدء الجلسة
        [HttpGet]
        public async Task<IActionResult> Start(string? deviceId)
        {
            var activeShift = await _shiftServices.GetActiveShiftAsync();
            if (activeShift == null)
            {
                TempData["ErrorMessage"] = "يجب فتح وردية أولاً لتشغيل الجلسات!";
                return RedirectToAction("OpenShift", "Shifts");
            }

            var allDevices = await _deviceService.GetAllAsync();
            var allCustomers = await _customerService.GetAllAsync();

            var model = new StartSessionViewModel
            {
                DeviceId = deviceId ?? string.Empty,
                // تصحيح: d.IsActive لعرض الأجهزة المتاحة داخل القائمة
                AvailableDevices = allDevices
                    .Where(d => d.IsActive || d.Id == deviceId)
                    .ToList(),
                Customers = allCustomers
                    .OrderBy(c => c.Name)
                    .ToList()
            };

            return View(model);
        }

        // 2. معالجة وحفظ الجلسة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(StartSessionViewModel model)
        {
            var activeShift = await _shiftServices.GetActiveShiftAsync();
            if (activeShift == null)
            {
                TempData["ErrorMessage"] = "الوردية مغلقة حالياً!";
                return RedirectToAction("OpenShift", "Shifts");
            }

            var device = await _deviceService.GetByIdAsync( model.DeviceId);
            if (device == null || !device.IsActive)
            {
                TempData["ErrorMessage"] = "هذا الجهاز غير متاح أو مشغول حالياً!";
                return RedirectToAction("Index", "Dashboard");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var session = new Session
            {
                DeviceId = model.DeviceId,
                CustomerId = model.CustomerId,
                ShiftId = activeShift.Id,
                UserId = currentUserId,
                StartTime = DateTime.Now,
                IsOpen = true,
                Mode = model.Mode,
                SessionType = model.SessionType,
                TargetMinutes = model.SessionType == "Limit" ? model.TargetMinutes : 0
            };

            // تحويل الجهاز إلى مشغول (غير متاح)
            device.IsActive = false;
            await _deviceService.Update(device);

            // حفظ الجلسة الجديدة
            await _sessionService.AddAsync(session);

            TempData["SuccessMessage"] = $"تم بدء الجلسة بنجاح على {device.Name}.";
            return RedirectToAction("Index", "Dashboard");
        }

        // 3. إضافة عميل سريع بدون مغادرة الصفحة (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCreateCustomer(string name, string? phone)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "برجاء إدخال اسم العميل" });
            }

            var customer = new Customer
            {
                Name = name.Trim(),
                Phone = phone?.Trim()
            };

            await _customerService.AddAsync(customer);

            return Json(new
            {
                success = true,
                id = customer.Id,
                displayText = $"{customer.Name} {(string.IsNullOrEmpty(customer.Phone) ? "" : "- " + customer.Phone)}"
            });
        }
        // 1. شاشة عرض تفاصيل الفاتورة وإضافة البوفيه
        [HttpGet]
        public async Task<IActionResult> End(string id)
        {
            var session = await _sessionService.GetByIdAsync(id);
            if (session == null || !session.IsOpen)
            {
                TempData["ErrorMessage"] = "الجلسة غير موجودة أو مغلقة مسبقاً!";
                return RedirectToAction("Index", "Dashboard");
            }

            var device = await _deviceService.GetByIdAsync(session.DeviceId);
            var customer = !string.IsNullOrEmpty(session.CustomerId)
                ? await _customerService.GetByIdAsync(session.CustomerId)
                : null;

            var endTime = DateTime.Now;
            var duration = endTime - session.StartTime;
            decimal totalMinutes = (decimal)duration.TotalMinutes;

            decimal hourlyRate = session.Mode == "Multi"
                ? (device?.HourPriceMulti ?? 0)
                : (device?.HourPriceSingle ?? 0);

            decimal deviceCost = Math.Round((totalMinutes / 60m) * hourlyRate, 2);

            // جلب المنتجات المتاحة في المخزن (شيبسي، مشروبات...)
            var products = (await _productService.GetAllAsync())
                .Where(p => p.StockQuantity > 0)
                .Select(p => new ProductItemViewModel
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Price = p.SellingPrice,
                    StockQuantity = p.StockQuantity
                }).ToList();

            var model = new EndSessionViewModel
            {
                SessionId = session.Id,
                DeviceName = device?.Name ?? "غير معروف",
                CustomerName = customer?.Name ?? "زبون عابر",
                StartTime = session.StartTime,
                EndTime = endTime,
                Duration = duration,
                Mode = session.Mode,
                HourlyRate = hourlyRate,
                DeviceCost = deviceCost,
                AvailableProducts = products
            };

            return View(model);
        }

        // 2. حفظ الفاتورة وإنهاء الجلسة وخصم المخزون
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEnd(EndSessionViewModel model)
        {
            var session = await _sessionService.GetByIdAsync(model.SessionId);
            if (session == null || !session.IsOpen)
            {
                TempData["ErrorMessage"] = "الجلسة غير صحيحة أو تم إغلاقها مسبقاً!";
                return RedirectToAction("Index", "Dashboard");
            }

            var device = await _deviceService.GetByIdAsync(session.DeviceId);
            var endTime = DateTime.Now;
            var duration = endTime - session.StartTime;
            decimal totalMinutes = (decimal)duration.TotalMinutes;

            decimal hourlyRate = session.Mode == "Multi"
                ? (device?.HourPriceMulti ?? 0)
                : (device?.HourPriceSingle ?? 0);

            decimal deviceCost = Math.Round((totalMinutes / 60m) * hourlyRate, 2);
            decimal productsCost = 0;

            // معالجة المنتجات والطلبات وحفظها
            if (model.SelectedOrders != null && model.SelectedOrders.Any())
            {
                foreach (var order in model.SelectedOrders.Where(o => o.Quantity > 0))
                {
                    var product = await _productService.GetByIdAsync(order.ProductId);
                    if (product != null)
                    {
                        var itemTotal = product.SellingPrice * order.Quantity;
                        productsCost += itemTotal;

                        // إنشاء سجل الطلب
                        var sessionOrder = new SessionOrder
                        {
                            SessionId = session.Id,
                            ProductId = product.Id,
                            Quantity = order.Quantity,
                            UnitPrice = product.SellingPrice
                        };
                        await _sessionOrderService.AddAsync(sessionOrder);

                        // خصم الكمية من المخزون
                        product.StockQuantity = Math.Max(0, product.StockQuantity - order.Quantity);
                        await _productService.Update(product);
                    }
                }
            }

            decimal totalFinal = (deviceCost + productsCost) - model.DiscountAmount;

            // تحديث بيانات الجلسة
            session.EndTime = endTime;
            session.IsOpen = false;
            session.IsPaid = true;
            session.DeviceCost = deviceCost;
            session.ProductsCost = productsCost;
            session.DiscountAmount = model.DiscountAmount;
            session.TotalAmount = totalFinal < 0 ? 0 : totalFinal;
            await _sessionService.Update(session);

            // إعادة إتاحة الجهاز
            if (device != null)
            {
                device.IsActive = true;
                await _deviceService.Update(device);
            }

            TempData["SuccessMessage"] = $"تم إنهاء جلسة {device?.Name} بنجاح. إجمالي الحساب: {session.TotalAmount:0.00} ج.م";
            return RedirectToAction("Index", "Dashboard");
        }
        public async Task<IActionResult> Details(string sessionId)
        {
            var session = await _sessionService.GetByIdAsync(sessionId);
            if (session == null)
            {
                TempData["ErrorMessage"] = "الجلسة غير موجودة!";
                return RedirectToAction("Index", "Dashboard");
            }
            var device = await _deviceService.GetByIdAsync(session.DeviceId);
            var customer = await _customerService.GetByIdAsync(session.CustomerId);
            var model = new SessionDetailsViewModel
            {
                Session = session,
                DeviceName = device?.Name ?? "غير معروف",
                CustomerName = customer?.Name ?? "غير معروف"
            };
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ActiveSessions()
        {
            var openSessions = (await _sessionService.GetAllAsync()).Where(s => s.IsOpen).ToList();
            var allDevices = (await _deviceService.GetAllAsync()).ToList();
            var allCustomers = (await _customerService.GetAllAsync()).ToList();

            var model = openSessions.Select(s => {
                var dev = allDevices.FirstOrDefault(d => d.Id == s.DeviceId);
                var cust = allCustomers.FirstOrDefault(c => c.Id == s.CustomerId);

                return new ActiveSessionViewModel
                {
                    SessionId = s.Id,
                    DeviceId = s.DeviceId,
                    DeviceName = dev?.Name ?? "غير معروف",
                    DeviceType = dev?.Type ?? "PS",
                    CustomerName = cust?.Name ?? "زبون عابر",
                    StartTime = s.StartTime,
                    Mode = s.Mode,
                    SessionType = s.SessionType,
                    TargetMinutes = s.TargetMinutes,
                    HourPriceSingle = dev?.HourPriceSingle ?? 0,
                    HourPriceMulti = dev?.HourPriceMulti ?? 0,
                    ProductsCost = s.ProductsCost
                };
            }).ToList();

            return View(model);
        }
    }
}