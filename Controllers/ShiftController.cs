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
    public class ShiftController : Controller
    {
        private readonly IGenericService<Shifts> _shiftService;
        private readonly IGenericService<Expense> _expenseService;
        private readonly IShiftServices _shiftServices;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShiftController(
            IGenericService<Shifts> shiftService,
            IGenericService<Expense> expenseService,
            IShiftServices shiftServices,
            ICurrentTenantService currentTenantService,
            UserManager<ApplicationUser> userManager)
        {
            _shiftService = shiftService;
            _expenseService = expenseService;
            _shiftServices = shiftServices;
            _currentTenantService = currentTenantService;
            _userManager = userManager;
        }

        private async Task<string> GetCurrentTenantIdAsync()
        {
            var tenantId = _currentTenantService.TenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                var user = await _userManager.GetUserAsync(User);
                tenantId = user?.TenantId ?? string.Empty;
            }
            return tenantId;
        }

        [HttpGet]
        public async Task<IActionResult> OpenShift()
        {
            var tenantId = await GetCurrentTenantIdAsync();

            // فحص هل يوجد وردية مفتوحة حالياً لنفس الفرع فقط
            var openShifts = await _shiftService.FindAsync(s => s.IsOpen && s.TenantId == tenantId);
            if (openShifts.Any())
            {
                TempData["ErrorMessage"] = "يوجد وردية مفتوحة بالفعل! يجب إغلاقها أولاً قبل فتح وردية جديدة.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenShift(decimal startingCash = 0)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var tenantId = await GetCurrentTenantIdAsync();

            // التأكد من عدم وجود وردية مفتوحة في نفس الفرع
            var openShifts = await _shiftService.FindAsync(s => s.IsOpen && s.TenantId == tenantId);
            if (openShifts.Any())
            {
                TempData["ErrorMessage"] = "يوجد وردية مفتوحة بالفعل في الصالة، يجب إغلاقها أولاً.";
                return RedirectToAction("Index", "Home");
            }

            var newShift = new Shifts
            {
                UserId = currentUserId,
                StartTime = DateTime.UtcNow,
                IsOpen = true,
                StartingCash = startingCash,
                ExpectedCash = startingCash,
                TenantId = tenantId
            };

            await _shiftService.AddAsync(newShift);
            await _shiftService.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم فتح الوردية بنجاح.";
            return RedirectToAction(nameof(GetReporet));
        }

        [HttpGet]
        public async Task<IActionResult> CloseShift()
        {
            var tenantId = await GetCurrentTenantIdAsync();
            var activeShift = (await _shiftService.FindAsync(s => s.IsOpen && s.TenantId == tenantId)).FirstOrDefault();

            if (activeShift == null)
            {
                TempData["ErrorMessage"] = "لا توجد وردية مفتوحة حالياً لإغلاقها.";
                return RedirectToAction("Index", "Home");
            }

            var cashierName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "الكاشير";
            var model = await _shiftServices.PrepareCloseShiftSummaryAsync(activeShift, cashierName);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseShift(CloseShiftViewModel model)
        {
            var shift = await _shiftService.GetByIdAsync(model.ShiftId);
            if (shift == null || !shift.IsOpen)
            {
                return NotFound();
            }

            shift.TotalGamingIncome = model.TotalGamingIncome;
            shift.TotalBuffetIncome = model.TotalBuffetIncome;
            shift.TotalDebtCollected = model.TotalDebtCollected;
            shift.TotalExpenses = model.TotalExpenses;

            decimal expectedCash = (shift.StartingCash + model.TotalGamingIncome + model.TotalBuffetIncome + model.TotalDebtCollected) - model.TotalExpenses;
            decimal variance = model.ActualCash - expectedCash;

            shift.EndTime = DateTime.UtcNow;
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
            var tenantId = await GetCurrentTenantIdAsync();
            var activeShift = (await _shiftService.FindAsync(s => s.IsOpen && s.TenantId == tenantId)).FirstOrDefault();

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

            var tenantId = await GetCurrentTenantIdAsync();
            var activeShift = (await _shiftService.FindAsync(s => s.IsOpen && s.TenantId == tenantId)).FirstOrDefault();

            if (activeShift == null)
            {
                TempData["ErrorMessage"] = "لا توجد وردية مفتوحة حالياً!";
                return RedirectToAction("Index", "Home");
            }

            var expense = new Expense
            {
                Title = title.Trim(),
                Amount = amount,
                Notes = notes,
                ShiftId = activeShift.Id,
                CreatedAt = DateTime.UtcNow,
                TenantId = tenantId
            };

            await _expenseService.AddAsync(expense);

            activeShift.TotalExpenses += amount;
            await _shiftService.Update(activeShift);

            await _expenseService.SaveChangesAsync();
            await _shiftService.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تسجيل المصروف وخصمه من رصيد الدرج بنجاح.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> GetReporet()
        {
            var shifts = await _shiftServices.GetDescShift();
            return View(shifts);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}