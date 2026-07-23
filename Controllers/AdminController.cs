using Microsoft.AspNetCore.Mvc;
using PlaystationSystem.Models;
using PlaystationSystem.Services;

namespace PlaystationSystem.Controllers
{
    public class AdminController : Controller
    {
        IAdminServices _adminServices;
        public AdminController(IAdminServices adminServices)
        {
            _adminServices = adminServices;

        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = _adminServices.GetAllUsersAsync().Result;
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
        public async Task<IActionResult> CreateUser(User user )
        {

            if (ModelState.IsValid)
            {
                await _adminServices.AddUserAsync(user);
                return RedirectToAction("Index");
            }
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _adminServices.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            await _adminServices.DeleteUserAsync(id);
            return RedirectToAction("Index");
        }
        [HttpPost]
       
        public async Task<IActionResult> EditUser(User user)
        {
            if (user==null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
             
         var User=   await _adminServices.UpdateUserAsync(user);
                return RedirectToAction("Index");
            }


            return View(User);
        }
        [HttpGet]
        
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _adminServices.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        [HttpGet]
        public async Task<IActionResult> GetDetailsForUser(int id)
        {
            var user =await _adminServices.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
    }
}
