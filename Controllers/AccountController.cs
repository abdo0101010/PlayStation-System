using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlaystationSystem.Models;
using PlaystationSystem.ViewModel;

namespace PlaystationSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager) { 

            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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

            var newUser = new ApplicationUser
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                IsActive = true // 👈 ضمان تفعيل الحساب فور إنشائه
            };

            var result = await _userManager.CreateAsync(newUser, user.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, "Cashier"); 

                return RedirectToAction("Index", "Home");
            }

  
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user); 
        }
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVIewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(loginViewModel.UserName);
               
                if (user != null) {
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "هذا الحساب معطل حالياً، يرجى مراجعة الأدمن.");
                        return View(loginViewModel);
                    }
                  var result = await _userManager.CheckPasswordAsync(user, loginViewModel.Password);
                    if (result)
                    {
                        await _signInManager.SignInAsync(user, loginViewModel.RememberMe);

                        return RedirectToAction("OpenShift", "Shift");
                    }
                    else
                    {
                        
                        ModelState.AddModelError("", "Invalid password.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "User not found.");
                }
            }
            return View(loginViewModel);
        }
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpGet]
        public IActionResult RolesList()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        // 2. شاشة إضافة رول جديدة
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        // 3. استقبال وحفظ الرول
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var roleExist = await _roleManager.RoleExistsAsync(model.RoleName.Trim());
            if (roleExist)
            {
                ModelState.AddModelError(string.Empty, "هذه الرول موجودة بالفعل!");
                return View(model);
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(model.RoleName.Trim()));
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"تمت إضافة الرول ({model.RoleName}) بنجاح.";
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
        public async Task<IActionResult> CreateAdmin(RegisterViewModel model, string selectedRole)
        {
            if (ModelState.IsValid)
            {
                // 1. التأكد أولاً من عدم تكرار اسم المستخدم أو الإيميل
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
                    // 2. التأكد من وجود الرتبة وإنشائها إذا لم تكن موجودة
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

                // 3. إضافة أخطاء Identity (مثل شروط قوة كلمة المرور)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

    }
}
