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
    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        IAdminServices _adminServices;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGenericRepository<Customer> _customerRepository;
        private readonly IGenericService<Shifts> _shiftService;
        private readonly IGenericService<DebtPayment> _debtPaymentService;

        public AdminController(IAdminServices adminServices, UserManager<ApplicationUser> userManager, IGenericRepository<Customer> customerRepository, IGenericService<Shifts> shiftService, IGenericService<DebtPayment> debtPaymentService)
        {
            _adminServices = adminServices;
            _userManager = userManager;
            _customerRepository = customerRepository;
            _shiftService = shiftService;
            _debtPaymentService = debtPaymentService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var shifts = _adminServices.GetAllShiftsAsync().Result;
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
        public async Task<IActionResult> CreateUser(ApplicationUser user)
        {

            if (ModelState.IsValid)
            {
                await _userManager.CreateAsync(user);
                return RedirectToAction("Index");
            }
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            await _userManager.DeleteAsync(user);
            return RedirectToAction("Index");
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

            if (ModelState.IsValid)
            {
                // تحديث البيانات المطلوبة
                existingUser.FullName = model.FullName;
                existingUser.UserName = model.UserName;
                existingUser.Email = model.Email;
                existingUser.PhoneNumber = model.PhoneNumber;
                existingUser.IsActive = model.IsActive;

                var result = await _userManager.UpdateAsync(existingUser);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "تم تحديث بيانات المستخدم بنجاح.";
                    return RedirectToAction("UsersList"); // أو الأكشن الذي يعرض المستخدمين
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
                return View(model);
        }
            [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
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
        public async Task<IActionResult> CreateCustomer(Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _customerRepository.AddAsync(customer);
                return RedirectToAction("GetAllCustomers");
            }
            return View(customer);
        }
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            await _customerRepository.Delete(customer);
            await _customerRepository.SaveChangesAsync();
            return RedirectToAction("GetAllCustomers");

        }
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            // هنا يتم جلب الجلسات والطلبات وسندات القبض المرتبطة بالعميل
            var statementModel = new CustomerStatementViewModel
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                PhoneNumber = customer.Phone,
                CurrentDebt = customer.Debt,
                Points = customer.TotalPoints,
                // مثال لملء العمليات (يمكن ربطها بجداول Sessions / Orders لاحقاً)
                Transactions = new List<CustomerTransactionViewModel>()
            };

            return View("CustomerStatement", statementModel);
        }
        [HttpGet]
        public async Task<IActionResult> PayDebt(string id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            if (customer.Debt <= 0)
            {
                TempData["InfoMessage"] = "هذا العميل ليس عليه أي مديونيات حالياً.";
                return RedirectToAction("GetAllCustomers");
            }

            return View(customer);
        }
        [HttpPost]
        public async Task<IActionResult> PayDebt(string customerId, decimal amountPaid)
        {
            if (amountPaid <= 0)
            {
                TempData["ErrorMessage"] = "المبلغ المدفوع يجب أن يكون أكبر من الصفر.";
                return RedirectToAction("GetAllCustomers");
            }

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null) return NotFound();

            // 1. خصم المبلغ من دين العميل
            customer.Debt -= amountPaid;
            if (customer.Debt < 0) customer.Debt = 0;

            await _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync();

            // 2. جلب الوردية المفتوحة
            var activeShift = (await _shiftService.FindAsync(s => s.IsOpen)).FirstOrDefault();

            // 3. حفظ حركة السداد في جدول DebtPayment (السطر الناقص)
            var debt = new DebtPayment
            {
                CustomerId = customerId,
                Amount = amountPaid,
                PaymentDate = DateTime.Now,
                ShiftId = activeShift?.Id
            };

            await _debtPaymentService.AddAsync(debt); // أو _context.DebtPayments.AddAsync(debt);
            await _debtPaymentService.SaveChangesAsync();

            // 4. تحديث إجمالي الوردية النشطة
            if (activeShift != null)
            {
                activeShift.TotalDebtCollected += amountPaid;
                await _shiftService.Update(activeShift);
                await _shiftService.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"تم تحصيل مبلغ {amountPaid:N2} ج.م من العميل {customer.Name} وإضافته لدرج الوردية.";
            return RedirectToAction("GetAllCustomers");
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

            if (ModelState.IsValid)
            {
                customer.Name = model.Name;
                customer.Phone = model.Phone;
                customer.Debt = model.Debt;
                customer.TotalPoints = model.TotalPoints;

                await _customerRepository.Update(customer);
                TempData["SuccessMessage"] = "تم تحديث بيانات العميل بنجاح.";
                return RedirectToAction(nameof(GetAllCustomers));
            }

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> GetDebtList()
        {
            var debtPayments = await _debtPaymentService.GetAllAsync();
            return View(debtPayments);
        }
    }

}
