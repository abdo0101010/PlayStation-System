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
                await _userManager.AddToRoleAsync(newUser, "Casiher"); 

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

                        return RedirectToAction("OpenShift", "Shifts");
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

    }
}
