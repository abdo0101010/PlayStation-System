using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlaystationSystem.Enums;
using PlaystationSystem.Models;
using PlaystationSystem.ViewModel;

namespace PlaystationSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class TenantManagementController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TenantManagementController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateTenantViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTenantViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. حساب تاريخ الانتهاء بناءً على نوع الباقة
            DateTime? endDate = model.SubscriptionType switch
            {
                SubscriptionType.Lifetime => null,
                SubscriptionType.Yearly => DateTime.UtcNow.AddYears(model.DurationInMonths > 0 ? model.DurationInMonths : 1),
                _ => DateTime.UtcNow.AddMonths(model.DurationInMonths > 0 ? model.DurationInMonths : 1)
            };

            // 2. إنشاء الـ Tenant
            var newTenant = new Tenant
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.StoreName,
                OwnerName = model.OwnerName,
                Phone = model.Phone,
                IsActive = true,
                SubscriptionType = model.SubscriptionType,
                SubscriptionEndDate = endDate,
                CreatedAt = DateTime.UtcNow
            };

            // 3. إنشاء المستخدم وربط قيم الاشتراك به مباشرة
            var adminUser = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.OwnerName,
                PhoneNumber = model.Phone,
                TenantId = newTenant.Id,
                IsActive = true,
                EmailConfirmed = true,
                // حفظ بيانات الاشتراك في المستخدم ليقرأها جدول العرض
                SubscriptionType = model.SubscriptionType,
                SubscriptionEndDate = endDate
            };

            var userResult = await _userManager.CreateAsync(adminUser, model.Password);

            if (!userResult.Succeeded)
            {
                foreach (var error in userResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // 4. إسناد دور الأدمن
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            await _userManager.AddToRoleAsync(adminUser, "Admin");

            // 5. حفظ الـ Tenant
            await _context.Tenants.AddAsync(newTenant);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم إنشاء المحل '{newTenant.Name}' وحساب الأدمن بنجاح.";
            return RedirectToAction("Index", "AdminManagement");
        }

    }
}
