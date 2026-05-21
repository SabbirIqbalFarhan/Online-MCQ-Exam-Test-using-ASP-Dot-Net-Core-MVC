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

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var totalResults = _context.Results.Where(r => r.UserId == user.Id).ToList();
            var permission = _context.ExamPermissions.FirstOrDefault(p => p.UserId == user.Id);
            var examRequest = _context.ExamRequests.FirstOrDefault(r => r.UserId == user.Id);
            var activeSession = _context.ExamSessions
    .Where(s => s.IsActive)
    .OrderByDescending(s => s.StartTime)
    .FirstOrDefault()
    ?? _context.ExamSessions
    .Where(s => s.StartTime <= DateTime.Now && s.EndTime >= DateTime.Now)
    .OrderByDescending(s => s.StartTime)
    .FirstOrDefault();
            var allSessions = _context.ExamSessions.OrderByDescending(s => s.CreatedAt).ToList();
            ViewBag.AllSessions = allSessions;
            var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == user.Id);

            ViewBag.UserName = user.UserName;
            ViewBag.TotalAttempts = totalResults.Count;
            ViewBag.Permission = permission;
            ViewBag.ExamRequest = examRequest;
            ViewBag.ActiveSession = activeSession;
            ViewBag.Profile = profile;
            var completedModules = _context.Results
    .Where(r => r.UserId == user.Id)
    .Select(r => r.ModuleId)
    .Distinct()
    .ToList();

            ViewBag.CompletedModules = completedModules;


            return View();
        }

        public async Task<IActionResult> RequestExamAccess()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var existing = _context.ExamRequests.FirstOrDefault(r => r.UserId == user.Id);

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

        public IActionResult Modules()
        {
            var modules = _context.Modules.ToList();
            return View(modules);
        }

        public async Task<IActionResult> Results()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var results = _context.Results
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.Id)
                .ToList();

            var sessions = _context.ExamSessions.ToList();
            ViewBag.Sessions = sessions;

            return View(results);
        }

        public IActionResult ChangePassword() => View();

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            ViewBag.Message = result.Succeeded ? "Password changed successfully!" : "Failed to change password!";
            return View();
        }
    }
}