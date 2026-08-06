using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaystationSystem.Models;
using PlaystationSystem.Services;

namespace PlaystationSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        IAdminServices _adminServices;
        private readonly UserManager<ApplicationUser> _userManager;


        public AdminController(IAdminServices adminServices, UserManager<ApplicationUser> userManager)
        {
            _adminServices = adminServices;
            _userManager = userManager;
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
        public async Task<IActionResult> CreateUser(ApplicationUser user )
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
            if (user==null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
             
         var User=   await _userManager.FindByIdAsync(user.Id);
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
            var user =await _userManager.FindByIdAsync(id);       
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
    }
}
