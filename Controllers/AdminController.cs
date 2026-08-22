using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaystationSystem.Models;
using PlaystationSystem.Repositoriy;
using PlaystationSystem.Services;
using PlaystationSystem.ViewModel;

namespace PlaystationSystem.Controllers
{
    [Authorize] // متاح لجميع الأدوار المسجلة (Admin, SuperAdmin, Cashier)
    public class AdminController : Controller
    {
        private readonly IAdminServices _adminServices;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGenericRepository<Customer> _customerRepository;
        private readonly IGenericService<Shifts> _shiftService;
        private readonly IGenericService<DebtPayment> _debtPaymentService;
        private readonly ICurrentTenantService _currentTenantService;

        public AdminController(
            IAdminServices adminServices,
            UserManager<ApplicationUser> userManager,
            IGenericRepository<Customer> customerRepository,
            IGenericService<Shifts> shiftService,
            IGenericService<DebtPayment> debtPaymentService,
            ICurrentTenantService currentTenantService)
        {
            _adminServices = adminServices;
            _userManager = userManager;
            _customerRepository = customerRepository;
            _shiftService = shiftService;
            _debtPaymentService = debtPaymentService;
            _currentTenantService = currentTenantService;
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

        // ======================== إدارة الموظفين (خاص بالمدير فقط) ========================

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Index()
        {
            var currentTenantId = await GetCurrentTenantIdAsync();
            var isSuperAdmin = _currentTenantService.IsSuperAdmin;

            var usersQuery = _userManager.Users.AsQueryable();
            if (!isSuperAdmin)
            {
                usersQuery = usersQuery.Where(u => u.TenantId == currentTenantId);
            }

            var users = await usersQuery.ToListAsync();
            var shifts = await _adminServices.GetAllShiftsAsync();

            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalShifts = shifts.Count;

            return View(users);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult CreateUser()
        {
            var model = new RegisterViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var tenantId = await GetCurrentTenantIdAsync();

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    TenantId = tenantId,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Cashier");

                    TempData["SuccessMessage"] = "تم إضافة الكاشير بنجاح.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentTenantId = await GetCurrentTenantIdAsync();
            if (!_currentTenantService.IsSuperAdmin && user.TenantId != currentTenantId)
            {
                return Forbid();
            }

            await _userManager.DeleteAsync(user);
            TempData["SuccessMessage"] = "تم حذف المستخدم بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentTenantId = await GetCurrentTenantIdAsync();
            if (!_currentTenantService.IsSuperAdmin && user.TenantId != currentTenantId)
            {
                return Forbid();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> EditUser(string id, ApplicationUser model)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            var currentTenantId = await GetCurrentTenantIdAsync();
            if (!_currentTenantService.IsSuperAdmin && existingUser.TenantId != currentTenantId)
            {
                return Forbid();
            }

            ModelState.Remove(nameof(model.TenantId));

            if (ModelState.IsValid)
            {
                existingUser.FullName = model.FullName;
                existingUser.UserName = model.UserName;
                existingUser.Email = model.Email;
                existingUser.PhoneNumber = model.PhoneNumber;
                existingUser.IsActive = model.IsActive;

                var result = await _userManager.UpdateAsync(existingUser);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "تم تحديث بيانات المستخدم بنجاح.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetDetailsForUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // ======================== إدارة العملاء والمديونيات (متاحة للكاشير والأدمن) ========================

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _customerRepository.GetAllAsync();
            return View(customers);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerDetails(string id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpGet]
        public IActionResult CreateCustomer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomer(Customer customer)
        {
            ModelState.Remove(nameof(customer.TenantId));

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(customer.TenantId))
                {
                    customer.TenantId = await GetCurrentTenantIdAsync();
                }

                await _customerRepository.AddAsync(customer);
                await _customerRepository.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إضافة العميل بنجاح.";
                return RedirectToAction(nameof(GetAllCustomers));
            }

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            await _customerRepository.Delete(customer);
            await _customerRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف العميل بنجاح.";
            return RedirectToAction(nameof(GetAllCustomers));
        }

        [HttpGet]
        public async Task<IActionResult> EditCustomer(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(string id, Customer model)
        {
            if (id != model.Id) return NotFound();

            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return NotFound();

            ModelState.Remove(nameof(model.TenantId));

            if (ModelState.IsValid)
            {
                customer.Name = model.Name;
                customer.Phone = model.Phone;
                customer.Debt = model.Debt;
                customer.TotalPoints = model.TotalPoints;

                await _customerRepository.Update(customer);
                await _customerRepository.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تحديث بيانات العميل بنجاح.";
                return RedirectToAction(nameof(GetAllCustomers));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var statementModel = new CustomerStatementViewModel
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                PhoneNumber = customer.Phone,
                CurrentDebt = customer.Debt,
                Points = customer.TotalPoints,
                Transactions = new List<CustomerTransactionViewModel>()
            };

            return View("CustomerStatement", statementModel);
        }

        [HttpGet]
        public async Task<IActionResult> PayDebt(string id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return NotFound();

            if (customer.Debt <= 0)
            {
                TempData["InfoMessage"] = "هذا العميل ليس عليه أي مديونيات حالياً.";
                return RedirectToAction(nameof(GetAllCustomers));
            }

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayDebt(string customerId, decimal amountPaid)
        {
            if (amountPaid <= 0)
            {
                TempData["ErrorMessage"] = "المبلغ المدفوع يجب أن يكون أكبر من الصفر.";
                return RedirectToAction(nameof(GetAllCustomers));
            }

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null) return NotFound();

            var tenantId = await GetCurrentTenantIdAsync();

            customer.Debt -= amountPaid;
            if (customer.Debt < 0) customer.Debt = 0;

            await _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync();

            var activeShift = (await _shiftService.FindAsync(s => s.IsOpen && s.TenantId == tenantId)).FirstOrDefault();

            var debt = new DebtPayment
            {
                CustomerId = customerId,
                Amount = amountPaid,
                PaymentDate = DateTime.UtcNow,
                ShiftId = activeShift?.Id,
                TenantId = tenantId
            };

            await _debtPaymentService.AddAsync(debt);
            await _debtPaymentService.SaveChangesAsync();

            if (activeShift != null)
            {
                activeShift.TotalDebtCollected += amountPaid;
                await _shiftService.Update(activeShift);
                await _shiftService.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"تم تحصيل مبلغ {amountPaid:N2} ج.م من العميل {customer.Name} بنجاح.";
            return RedirectToAction(nameof(GetAllCustomers));
        }

        [HttpGet]
        public async Task<IActionResult> GetDebtList()
        {
            var debtPayments = await _debtPaymentService.GetAllAsync();
            return View(debtPayments);
        }
    }
}