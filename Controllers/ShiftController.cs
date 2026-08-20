using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaystationSystem.Models;
using PlaystationSystem.Services;
using PlaystationSystem.ViewModel;

namespace PlaystationSystem.Controllers
{
    public class ShiftController : Controller
    {
        private readonly IGenericService<Shifts> _shiftService;
        private readonly IGenericService<Expense> _expenseService;
        private readonly IShiftServices _shiftServices;
        public ShiftController(IGenericService<Shifts> shiftService, IGenericService<Expense> expenseService, IShiftServices shiftServices)
        {
            _expenseService = expenseService;

            _shiftService = shiftService;
            _shiftServices = shiftServices;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenShift(decimal startingCash = 0)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            // 1. التأكد من عدم وجود أي وردية مفتوحة في المحل حالياً
            var openShifts = await _shiftService.FindAsync(s => s.IsOpen);
            if (openShifts.Any())
            {
                TempData["ErrorMessage"] = "يوجد وردية مفتوحة بالفعل في السيستم، يجب إغلاقها أولاً.";
                return RedirectToAction(nameof(Index));
            }

            // 2. إنشاء الوردية الجديدة بالعهدة المدخلة
            var newShift = new Shifts
            {
                UserId = currentUserId,
                StartTime = DateTime.Now,
                IsOpen = true,
                StartingCash = startingCash,
                ExpectedCash = startingCash // تبدأ بنفس رصيد الافتتاح
            };

            await _shiftService.AddAsync(newShift);
            await _shiftService.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم فتح الوردية بنجاح.";
            return RedirectToAction("GetReporet");
        }
        [HttpGet]
        public async Task<IActionResult> OpenShift()
        {
            // فحص هل يوجد وردية مفتوحة حالياً في السيستم
            var openShifts = await _shiftService.FindAsync(s => s.IsOpen);
            if (openShifts.Any())
            {
                TempData["ErrorMessage"] = "يوجد وردية مفتوحة بالفعل! يجب إغلاقها أولاً قبل فتح وردية جديدة.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> CloseShift()
        {
            var activeShift = (await _shiftService.FindAsync(s => s.IsOpen)).FirstOrDefault();
            if (activeShift == null)
            {
                TempData["ErrorMessage"] = "لا توجد وردية مفتوحة حالياً لإغلاقها.";
                return RedirectToAction("Index", "Home");
            }

            var cashierName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "الكاشير";

            // استدعاء السيرفيس لحساب المجاميع الفعلية وتعبئة الموديل
            var model = await _shiftServices.PrepareCloseShiftSummaryAsync(activeShift, cashierName);

            return View(model);
        }

        // 2. معالجة الإغلاق (HttpPost) - استلام الأرقام الجاهزة من الحقول المخفية
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseShift(CloseShiftViewModel model)
        {
            var shift = await _shiftService.GetByIdAsync(model.ShiftId);
            if (shift == null || !shift.IsOpen)
            {
                return NotFound();
            }

            // إسناد الأرقام المستلمة من الحقول المخفية إلى كائن الوردية
            shift.TotalGamingIncome = model.TotalGamingIncome;
            shift.TotalBuffetIncome = model.TotalBuffetIncome;
            shift.TotalDebtCollected = model.TotalDebtCollected;
            shift.TotalExpenses = model.TotalExpenses;

            // حساب النقدية المتوقعة وفارق الخزينة
            decimal expectedCash = (shift.StartingCash + model.TotalGamingIncome + model.TotalBuffetIncome + model.TotalDebtCollected) - model.TotalExpenses;
            decimal variance = model.ActualCash - expectedCash;

            shift.EndTime = DateTime.Now;
            shift.ExpectedCash = expectedCash;
            shift.ActualCash = model.ActualCash;
            shift.ShortageOrSurplus = variance;
            shift.Notes = model.Notes;
            shift.IsOpen = false;

            await _shiftService.Update(shift);
            await _shiftService.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم إغلاق الوردية بنجاح وتسليم الخزينة.";
            return RedirectToAction("ShiftReport", new { id = shift.Id });
        }
        // 3. تقرير ملخص الوردية بعد الإغلاق مباشرة
        [HttpGet]
        public async Task<IActionResult> ShiftReport(string id)
        {
            var shift = await _shiftService.GetByIdAsync(id);
            if (shift == null) return NotFound();
            return View(shift);
        }
        [HttpGet]
        public async Task<IActionResult> AddExpense()
        {
            // التأكد من وجود وردية مفتوحة
            var activeShift = (await _shiftService.FindAsync(s => s.IsOpen)).FirstOrDefault();
            if (activeShift == null)
            {
                TempData["ErrorMessage"] = "يجب فتح وردية أولاً لتسجيل المصروفات!";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(string title, decimal amount, string? notes)
        {
            if (amount <= 0 || string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("", "برجاء إدخال بند الصرف ومبلغ صحيح أكبر من الصفر.");
                return View();
            }

            var activeShift = (await _shiftService.FindAsync(s => s.IsOpen)).FirstOrDefault();
            if (activeShift == null)
            {
                TempData["ErrorMessage"] = "لا توجد وردية مفتوحة حالياً!";
                return RedirectToAction("Index", "Home");
            }

            // 1. تسجيل حركة المصروف
            var expense = new Expense
            {
                Title = title,
                Amount = amount,
                Notes = notes,
                ShiftId = activeShift.Id,
                CreatedAt = DateTime.Now
            };
            if (ModelState.IsValid)
            {
                await _expenseService.AddAsync(expense);
            activeShift.TotalExpenses += amount;
            await _shiftService.Update(activeShift);

            await _expenseService.SaveChangesAsync();

            }

            // 2. تحديث إجمالي المصروفات في الوردية الحالية
            TempData["SuccessMessage"] = "تم تسجيل المصروف وخصمه من رصيد الدرج بنجاح.";
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> GetReporet()
        {
            // جلب جميع الورديات من الأحدث إلى الأقدم مع بيانات المستخدم
            var shifts = await _shiftServices.GetDescShift();

            return View(shifts);
        }

        public IActionResult Index()
        {
            return View();
        }


    }
}
