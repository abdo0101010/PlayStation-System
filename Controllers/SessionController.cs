using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlaystationSystem.Models;
using PlaystationSystem.Services;
using PlaystationSystem.ViewModel;
using System.Security.Claims;

namespace PlaystationSystem.Controllers
{
    [Authorize]
    public class SessionController : Controller
    {
        private readonly IGenericService<Session> _sessionService;
        private readonly IGenericService<Customer> _customerService;
        private readonly IGenericService<Device> _deviceService;
        private readonly IShiftServices _shiftServices;
        private readonly IGenericService<Product> _productService;
        private readonly IGenericService<SessionOrder> _sessionOrderService;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SessionController(
            IGenericService<Session> sessionService,
            IGenericService<Customer> customerService,
            IGenericService<Device> deviceService,
            IShiftServices shiftServices,
            IGenericService<Product> productService,
            IGenericService<SessionOrder> sessionOrderService,
            ICurrentTenantService currentTenantService,
            UserManager<ApplicationUser> userManager)
        {
            _sessionService = sessionService;
            _customerService = customerService;
            _deviceService = deviceService;
            _shiftServices = shiftServices;
            _productService = productService;
            _sessionOrderService = sessionOrderService;
            _currentTenantService = currentTenantService;
            _userManager = userManager;
        }

        // دالة موحدة لضبط توقيت مصر بدقة على أي سيرفر استضافة
        private static DateTime GetEgyptTime()
        {
            try
            {
                var timeZoneId = OperatingSystem.IsWindows() ? "Egypt Standard Time" : "Africa/Cairo";
                var egyptZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptZone);
            }
            catch
            {
                return DateTime.UtcNow.AddHours(3);
            }
        }

        // 1. فتح شاشة بدء الجلسة
        [HttpGet]
        public async Task<IActionResult> Start(string? deviceId)
        {
            var activeShift = await _shiftServices.GetActiveShiftAsync();
            if (activeShift == null)
            {
                TempData["ErrorMessage"] = "يجب فتح وردية أولاً لتشغيل الجلسات!";
                return RedirectToAction("OpenShift", "Shift");
            }

            var allDevices = await _deviceService.GetAllAsync();
            var allCustomers = await _customerService.GetAllAsync();

            var model = new StartSessionViewModel
            {
                DeviceId = deviceId ?? string.Empty,
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
                return RedirectToAction("OpenShift", "Shift");
            }

            var device = await _deviceService.GetByIdAsync(model.DeviceId);
            if (device == null || !device.IsActive)
            {
                TempData["ErrorMessage"] = "هذا الجهاز غير متاح أو مشغول حالياً!";
                return RedirectToAction("Index", "Dashboard");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var tenantId = _currentTenantService.TenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                var user = await _userManager.GetUserAsync(User);
                tenantId = user?.TenantId ?? string.Empty;
            }

            var session = new Session
            {
                DeviceId = model.DeviceId,
                CustomerId = model.CustomerId,
                ShiftId = activeShift.Id,
                UserId = currentUserId,
                StartTime = GetEgyptTime(), // استخدام توقيت مصر
                IsOpen = true,
                Mode = model.Mode,
                SessionType = model.SessionType,
                TargetMinutes = model.SessionType == "Limit" ? model.TargetMinutes : 0,
                TenantId = tenantId
            };

            // تحويل الجهاز إلى مشغول
            device.IsActive = false;
            await _deviceService.Update(device);

            // حفظ الجلسة الجديدة
            await _sessionService.AddAsync(session);

            TempData["SuccessMessage"] = $"تم بدء الجلسة بنجاح على {device.Name}.";
            return RedirectToAction("ActiveSessions");
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

            var tenantId = _currentTenantService.TenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                var user = await _userManager.GetUserAsync(User);
                tenantId = user?.TenantId ?? string.Empty;
            }

            var customer = new Customer
            {
                Name = name.Trim(),
                Phone = phone?.Trim(),
                TenantId = tenantId
            };

            await _customerService.AddAsync(customer);

            return Json(new
            {
                success = true,
                id = customer.Id,
                displayText = $"{customer.Name} {(string.IsNullOrEmpty(customer.Phone) ? "" : "- " + customer.Phone)}"
            });
        }

        // 4. شاشة عرض تفاصيل الفاتورة وإضافة البوفيه
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

            var endTime = GetEgyptTime();
            var duration = endTime - session.StartTime;
            if (duration.TotalSeconds < 0) duration = TimeSpan.Zero;
            decimal totalMinutes = (decimal)duration.TotalMinutes;

            decimal hourlyRate = session.Mode == "Multi"
                ? (device?.HourPriceMulti ?? 0)
                : (device?.HourPriceSingle ?? 0);

            decimal deviceCost = Math.Round((totalMinutes / 60m) * hourlyRate, 2);

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

        // 5. حفظ الفاتورة وإنهاء الجلسة وخصم المخزون
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
            var endTime = GetEgyptTime();
            var duration = endTime - session.StartTime;
            if (duration.TotalSeconds < 0) duration = TimeSpan.Zero;
            decimal totalMinutes = (decimal)duration.TotalMinutes;

            decimal hourlyRate = session.Mode == "Multi"
                ? (device?.HourPriceMulti ?? 0)
                : (device?.HourPriceSingle ?? 0);

            decimal deviceCost = Math.Round((totalMinutes / 60m) * hourlyRate, 2);
            decimal productsCost = 0;

            var tenantId = _currentTenantService.TenantId ?? session.TenantId;

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

                        var sessionOrder = new SessionOrder
                        {
                            SessionId = session.Id,
                            ProductId = product.Id,
                            Quantity = order.Quantity,
                            UnitPrice = product.SellingPrice,
                            TenantId = tenantId
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
            return RedirectToAction("Index", "Home");
        }

        // 6. تفاصيل الجلسة
        [HttpGet]
        public async Task<IActionResult> Details(string sessionId)
        {
            var session = await _sessionService.GetByIdAsync(sessionId);
            if (session == null)
            {
                TempData["ErrorMessage"] = "الجلسة غير موجودة!";
                return RedirectToAction("Index", "Home");
            }

            var device = await _deviceService.GetByIdAsync(session.DeviceId);
            var customer = !string.IsNullOrEmpty(session.CustomerId)
                ? await _customerService.GetByIdAsync(session.CustomerId)
                : null;

            // جلب الطلبات المرتبطة
            var orders = (await _sessionOrderService.FindAsync(so => so.SessionId == sessionId)).ToList();
            foreach (var order in orders)
            {
                order.Product = await _productService.GetByIdAsync(order.ProductId);
            }
            session.Orders = orders;

            if (session.IsOpen)
            {
                var now = GetEgyptTime();
                var duration = now - session.StartTime;
                if (duration.TotalSeconds < 0) duration = TimeSpan.Zero;

                decimal totalMinutes = (decimal)duration.TotalMinutes;
                decimal hourlyRate = session.Mode == "Multi"
                    ? (device?.HourPriceMulti ?? 0)
                    : (device?.HourPriceSingle ?? 0);

                if (session.SessionType == "Limit" && session.TargetMinutes > 0)
                {
                    totalMinutes = Math.Max(session.TargetMinutes, totalMinutes);
                }

                session.DeviceCost = Math.Round((totalMinutes / 60m) * hourlyRate, 2);
                session.ProductsCost = orders.Sum(o => o.UnitPrice * o.Quantity);
                decimal finalTotal = (session.DeviceCost + session.ProductsCost) - session.DiscountAmount;
                session.TotalAmount = finalTotal < 0 ? 0 : finalTotal;
            }

            var model = new SessionDetailsViewModel
            {
                Session = session,
                DeviceName = device?.Name ?? "غير معروف",
                CustomerName = customer?.Name ?? "زبون عابر"
            };
            return View(model);
        }

        // 7. الجلسات النشطة
        [HttpGet]
        public async Task<IActionResult> ActiveSessions()
        {
            var openSessions = (await _sessionService.GetAllAsync()).Where(s => s.IsOpen).ToList();
            var allDevices = (await _deviceService.GetAllAsync()).ToList();
            var allCustomers = (await _customerService.GetAllAsync()).ToList();

            var model = openSessions.Select(s =>
            {
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

        // 8. حذف الجلسة
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(string sessionId)
        {
            var session = await _sessionService.GetByIdAsync(sessionId);
            if (session == null)
            {
                TempData["ErrorMessage"] = "الجلسة غير موجودة!";
                return RedirectToAction("ActiveSessions");
            }

            var tenantId = _currentTenantService.TenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                var user = await _userManager.GetUserAsync(User);
                tenantId = user?.TenantId ?? string.Empty;
            }

            if (!_currentTenantService.IsSuperAdmin && session.TenantId != tenantId)
            {
                return Forbid();
            }

            // 1. إعادة إتاحة الجهاز إذا كانت الجلسة مفتوحة
            if (session.IsOpen)
            {
                var device = await _deviceService.GetByIdAsync(session.DeviceId);
                if (device != null)
                {
                    device.IsActive = true;
                    await _deviceService.Update(device);
                }
            }

            // 2. إرجاع بضاعة البوفيه إلى المخزون وحذف سجلات الطلبات
            var sessionOrders = await _sessionOrderService.FindAsync(so => so.SessionId == sessionId);
            if (sessionOrders != null && sessionOrders.Any())
            {
                foreach (var order in sessionOrders)
                {
                    var product = await _productService.GetByIdAsync(order.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += order.Quantity;
                        await _productService.Update(product);
                    }
                    await _sessionOrderService.Delete(order);
                }
            }

            // 3. حذف الجلسة نهائياً
            await _sessionService.Delete(session);

            TempData["SuccessMessage"] = "تم حذف الجلسة وإعادة الأصناف للمخزون بنجاح.";
            return RedirectToAction("ActiveSessions");
        }

        // 9. سجل الجلسات للآدمن
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> SessionHistory()
        {
            var tenantId = _currentTenantService.TenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                var user = await _userManager.GetUserAsync(User);
                tenantId = user?.TenantId ?? string.Empty;
            }

            var isSuperAdmin = _currentTenantService.IsSuperAdmin;

            var sessions = (await _sessionService.GetAllAsync())
                .Where(s => isSuperAdmin || s.TenantId == tenantId)
                .OrderByDescending(s => s.StartTime)
                .ToList();

            var devices = await _deviceService.GetAllAsync();
            var customers = await _customerService.GetAllAsync();
            var now = GetEgyptTime();

            var model = sessions.Select(s => {
                var dev = devices.FirstOrDefault(d => d.Id == s.DeviceId);
                var cust = customers.FirstOrDefault(c => c.Id == s.CustomerId);

                decimal finalAmount = s.TotalAmount;

                if (s.IsOpen)
                {
                    var duration = now - s.StartTime;
                    if (duration.TotalSeconds < 0) duration = TimeSpan.Zero;

                    decimal totalMinutes = (decimal)duration.TotalMinutes;
                    decimal hourlyRate = s.Mode == "Multi"
                        ? (dev?.HourPriceMulti ?? 0)
                        : (dev?.HourPriceSingle ?? 0);

                    if (s.SessionType == "Limit" && s.TargetMinutes > 0)
                    {
                        totalMinutes = Math.Max(s.TargetMinutes, totalMinutes);
                    }

                    decimal currentDeviceCost = Math.Round((totalMinutes / 60m) * hourlyRate, 2);
                    finalAmount = (currentDeviceCost + s.ProductsCost) - s.DiscountAmount;
                    if (finalAmount < 0) finalAmount = 0;
                }

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
                    ProductsCost = s.ProductsCost,
                    HourPriceSingle = finalAmount
                };
            }).ToList();

            return View(model);
        }
    }
}