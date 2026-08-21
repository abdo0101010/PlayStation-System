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
    [Authorize(Roles = "Admin,SuperAdmin")]
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentTenantId = _currentTenantService.TenantId;
            var isSuperAdmin = _currentTenantService.IsSuperAdmin;

            // جلب الموظفين التابعين لنفس المحل فقط (أو الكل إذا كان سوبر أدمن)
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
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(ApplicationUser user, string password, string selectedRole)
        {
            ModelState.Remove(nameof(user.TenantId));

            if (ModelState.IsValid)
            {
                // ربط الموظف بمحل الأدمن الحالي
                user.TenantId = _currentTenantService.TenantId;
                user.IsActive = true;
                user.EmailConfirmed = true;

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    var role = string.IsNullOrEmpty(selectedRole) ? "Cashier" : selectedRole;
                    await _userManager.AddToRoleAsync(user, role);

                    TempData["SuccessMessage"] = "تم إضافة المستخدم بنجاح.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // التحقق من أن المستخدم يتبع نفس المحل
            if (!_currentTenantService.IsSuperAdmin && user.TenantId != _currentTenantService.TenantId)
            {
                return Forbid();
            }

            await _userManager.DeleteAsync(user);
            TempData["SuccessMessage"] = "تم حذف المستخدم بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!_currentTenantService.IsSuperAdmin && user.TenantId != _currentTenantService.TenantId)
            {
                return Forbid();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, ApplicationUser model)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (!_currentTenantService.IsSuperAdmin && existingUser.TenantId != _currentTenantService.TenantId)
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
        public async Task<IActionResult> GetDetailsForUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // ======================== إدارة العملاء والمديونيات ========================

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
                    customer.TenantId = _currentTenantService.TenantId ?? string.Empty;
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

            // 1. خصم المبلغ من دين العميل
            customer.Debt -= amountPaid;
            if (customer.Debt < 0) customer.Debt = 0;

            await _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync();

            // 2. جلب الوردية المفتوحة التابعة لنفس المحل
            var activeShift = (await _shiftService.FindAsync(s => s.IsOpen)).FirstOrDefault();

            // 3. حفظ حركة السداد
            var debt = new DebtPayment
            {
                CustomerId = customerId,
                Amount = amountPaid,
                PaymentDate = DateTime.UtcNow,
                ShiftId = activeShift?.Id,
                TenantId = _currentTenantService.TenantId ?? string.Empty
            };

            await _debtPaymentService.AddAsync(debt);
            await _debtPaymentService.SaveChangesAsync();

            // 4. تحديث إجمالي الوردية النشطة
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