using Exam_Test.Data;
using Exam_Test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Test.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================
        // USER DASHBOARD
        // =========================
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var totalResults = _context.Results
                .Where(r => r.UserId == user.Id)
                .ToList();

            var permission = _context.ExamPermissions
                .FirstOrDefault(p => p.UserId == user.Id);

            var examRequest = _context.ExamRequests
                .FirstOrDefault(r => r.UserId == user.Id);

            ViewBag.UserName = user.UserName;
            ViewBag.TotalAttempts = totalResults.Count;
            ViewBag.Permission = permission;
            ViewBag.ExamRequest = examRequest;

            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        // =========================
        // REQUEST EXAM ACCESS
        // =========================
        public async Task<IActionResult> RequestExamAccess()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            // Check if already requested
            var existing = _context.ExamRequests
                .FirstOrDefault(r => r.UserId == user.Id);

            if (existing == null)
            {
                _context.ExamRequests.Add(new ExamRequest
                {
                    UserId = user.Id,
                    RequestedAt = DateTime.Now,
                    Status = "Pending"
                });
                _context.SaveChanges();
            }

            return RedirectToAction("Dashboard");
        }

        // =========================
        // MODULES PAGE
        // =========================
        public IActionResult Modules()
        {
            var modules = _context.Modules.ToList();
            return View(modules);
        }

        // =========================
        // USER RESULTS PAGE
        // =========================
        public async Task<IActionResult> Results()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var results = _context.Results
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.Id)
                .ToList();

            return View(results);
        }

        // =========================
        // CHANGE PASSWORD
        // =========================
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
                ViewBag.Message = "Password changed successfully!";
            else
                ViewBag.Message = "Failed to change password!";

            return View();
        }
    }
}