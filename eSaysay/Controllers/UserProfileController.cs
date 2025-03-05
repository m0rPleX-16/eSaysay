using eSaysay.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eSaysay.Models.ViewModels;
using eSaysay.Models.Entities;

namespace eSaysay.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Profile()
        {
           
            return View();
        }
    }

}
