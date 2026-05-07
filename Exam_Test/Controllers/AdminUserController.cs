using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Exam_Test.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminUserController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // =========================
        // ALL USERS
        // =========================
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // =========================
        // USER DETAILS
        // =========================
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.Roles = roles;

            return View(user);
        }
    }
}