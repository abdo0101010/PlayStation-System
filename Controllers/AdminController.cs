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

        public AdminController(IAdminServices adminServices, UserManager<ApplicationUser> userManager, IGenericRepository<Customer> customerRepository)
        {
            _adminServices = adminServices;
            _userManager = userManager;
            _customerRepository = customerRepository;
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

        public async Task<IActionResult> EditUser(ApplicationUser user)
        {
            if (user == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {

                var User = await _userManager.FindByIdAsync(user.Id);
                return RedirectToAction("Index");
            }


            return View(User);
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerDetails(int id)
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
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            await _customerRepository.Delete(customer);
            return RedirectToAction("GetAllCustomers");

        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
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
        public async Task<IActionResult> PayDebt(int id)
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
        public async Task<IActionResult> PayDebt(int customerId, decimal amountPaid)
        {
            if (amountPaid <= 0)
            {
                ModelState.AddModelError("", "المبلغ المدفوع يجب أن يكون أكبر من الصفر.");
                return RedirectToAction("GetAllCustomers");
            }

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null) return NotFound();

            // 1. خصم المبلغ من دين العميل
            customer.Debt -= amountPaid;
            if (customer.Debt < 0) customer.Debt = 0; // حماية من القيم السالبة

           await _customerRepository.Update(customer);

            // 2. (اختياري) إضافة المبلغ لحصيلة درج الوردية الحالية
            // await _shiftService.AddIncomeAsync(amountPaid, $"سداد دين من العميل: {customer.Name}");

            return RedirectToAction("GetAllCustomers");
        }
    }
}
