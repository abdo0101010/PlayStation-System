using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaystationSystem.Models;
using PlaystationSystem.Services;
using PlaystationSystem.ViewModel;

namespace PlaystationSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ICurrentTenantService _currentTenantService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ICurrentTenantService currentTenantService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _currentTenantService = currentTenantService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            // ربط الكاشير المسجل بالصالة الحالية
            var tenantId = _currentTenantService.TenantId;

            var newUser = new ApplicationUser
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                TenantId = tenantId,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newUser, user.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Cashier"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Cashier"));
                }

                await _userManager.AddToRoleAsync(newUser, "Cashier");
                TempData["SuccessMessage"] = "تم إنشاء الحساب بنجاح.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVIewModel loginViewModel, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // البحث عن المستخدم بدون الـ Global Filter لضمان جلبه سواء باسم المستخدم أو البريد
                var user = await _userManager.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.UserName == loginViewModel.UserName || u.Email == loginViewModel.UserName);

                if (user != null)
                {
                    // 1. التحقق من تفعيل حساب المستخدم الفردي
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "هذا الحساب معطل حالياً، يرجى مراجعة إدارة النظام.");
                        return View(loginViewModel);
                    }

                    var isSuperAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin");

                    // 2. إذا لم يكن SuperAdmin، نتحقق من حالة اشتراك المنشأة (Tenant)
                    if (!isSuperAdmin && !string.IsNullOrEmpty(user.TenantId))
                    {
                        var tenant = await _context.Tenants
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(t => t.Id == user.TenantId);

                        if (tenant == null || !tenant.IsActive)
                        {
                            ModelState.AddModelError(string.Empty, "عذراً، اشتراك هذه الصالة معطل حالياً، يرجى التواصل مع الإدارة للتجديد.");
                            return View(loginViewModel);
                        }

                        if (tenant.SubscriptionEndDate.HasValue && tenant.SubscriptionEndDate.Value < DateTime.UtcNow)
                        {
                            ModelState.AddModelError(string.Empty, "انتهت فترة اشتراك هذه الصالة. يرجى تجديد الاشتراك للمتابعة.");
                            return View(loginViewModel);
                        }
                    }

                    // 3. التحقق من كلمة المرور وتسجيل الدخول
                    var result = await _signInManager.PasswordSignInAsync(user.UserName!, loginViewModel.Password, loginViewModel.RememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        // توجيه السوبر أدمن لإدارة الاشتراكات والمحلات
                        if (isSuperAdmin)
                        {
                            return RedirectToAction("Index", "AdminManagement");
                        }

                        // توجيه الكاشير والأدمن لصفحة فتح الوردية
                        return RedirectToAction("OpenShift", "Shift");
                    }

                    ModelState.AddModelError(string.Empty, "كلمة المرور غير صحيحة.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "اسم المستخدم أو البريد الإلكتروني غير موجود.");
                }
            }

            return View(loginViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public IActionResult RolesList()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var roleName = model.RoleName.Trim();
            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (roleExist)
            {
                ModelState.AddModelError(string.Empty, "هذه الرول موجودة بالفعل!");
                return View(model);
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"تمت إضافة الرول ({roleName}) بنجاح.";
                return RedirectToAction(nameof(RolesList));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(RegisterViewModel model, string selectedRole)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByNameAsync(model.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError("UserName", "اسم المستخدم هذا مسجل بالفعل.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(selectedRole))
                    {
                        if (!await _roleManager.RoleExistsAsync(selectedRole))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(selectedRole));
                        }
                        await _userManager.AddToRoleAsync(user, selectedRole);
                    }

                    TempData["SuccessMessage"] = $"تم إنشاء حساب {selectedRole} بنجاح باسم {user.FullName}.";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}