using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using eSaysay.Models;
using System.Diagnostics;

namespace eSaysay.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ILogger<AdminController> logger, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View("~/Views/User/Admin/Index.cshtml");
        }
        public IActionResult Analytics()
        {
            return View("~/Views/User/Admin/Analytics.cshtml");
        }
        public IActionResult Exercises()
        {

            return View("~/Views/User/Admin/Exercises.cshtml");
        }
        public IActionResult Lessons()
        {
            return View("~/Views/User/Admin/Lessons.cshtml");
        }
        public IActionResult Logs()
        {
            return View("~/Views/User/Admin/Logs.cshtml");
        }
        public IActionResult Progress()
        {
            return View("~/Views/User/Admin/Progress.cshtml");
        }

        public async Task<IActionResult> Students()
        {
            var users = await _userManager.Users.ToListAsync();

            var students = new List<IdentityUser>();
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Student"))
                {
                    students.Add(user);
                }
            }

            return View("~/Views/User/Admin/Students.cshtml", students);
        }
        [HttpPost]
        public async Task<IActionResult> EditStudent(string Id, string Email)
        {
            if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Email))
            {
                return BadRequest("Invalid input.");
            }

            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Email = Email;
            user.UserName = Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Students");
            }

            return BadRequest("Failed to update user.");
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveStudent(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return BadRequest("Invalid input.");
            }

            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.MaxValue;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Students");
            }

            return BadRequest("Failed to archive user.");
        }


        public IActionResult Settings()
        {
            return View("~/Views/User/Admin/Settings.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
