using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlaystationSystem.Enums;
using PlaystationSystem.Models;

namespace PlaystationSystem.Controllers
{
    public class AdminManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminManagementController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // عرض جميع حسابات الأدمن مع حالة اشتراكاتهم
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            return View(admins);
        }

        // تفعيل / تعطيل الحساب بنقرة واحدة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive; // عكس الحالة الحالية
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"تم {(user.IsActive ? "تفعيل" : "تعطيل")} حساب {user.FullName} بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // تجديد أو تعديل خطة الاشتراك
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewSubscription(string userId, SubscriptionType type, int durationInMonths)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.SubscriptionType = type;
            user.SubscriptionStartDate = DateTime.UtcNow;
            user.IsActive = true;

            if (type == SubscriptionType.Lifetime)
            {
                user.SubscriptionEndDate = null;
            }
            else if (type == SubscriptionType.Yearly)
            {
                user.SubscriptionEndDate = DateTime.UtcNow.AddYears(1);
            }
            else // Monthly
            {
                user.SubscriptionEndDate = DateTime.UtcNow.AddMonths(durationInMonths > 0 ? durationInMonths : 1);
            }

            await _userManager.UpdateAsync(user);
            TempData["SuccessMessage"] = $"تم تحديث اشتراك {user.FullName} كـ {type}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
