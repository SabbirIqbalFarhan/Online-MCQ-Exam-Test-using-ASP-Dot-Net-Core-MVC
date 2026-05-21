using Exam_Test.Data;
using Exam_Test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Test.Controllers
{
    [Authorize(Roles = "User")]
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
            var completedModules = activeSession != null
    ? _context.Results
        .Where(r => r.UserId == user.Id && r.SessionId == activeSession.Id)
        .Select(r => r.ModuleId)
        .Distinct()
        .ToList()
    : new List<int>();

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

        public async Task<IActionResult> ViewResult(int sessionId, int moduleId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var now = DateTime.Now;
            var session = _context.ExamSessions
                .Where(s => s.IsActive && s.StartTime <= now && s.EndTime >= now)
                .OrderByDescending(s => s.StartTime)
                .FirstOrDefault();
            if (session == null) return NotFound();

            var result = _context.Results
                .FirstOrDefault(r => r.UserId == user.Id && r.SessionId == sessionId && r.ModuleId == moduleId);

            var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == user.Id);

            int totalCorrect = result?.Correct ?? 0;
            int totalWrong = result?.Wrong ?? 0;
            int totalQuestions = totalCorrect + totalWrong;

            var moduleQuestionIds = _context.Questions
                .Where(q => q.ModuleId == moduleId)
                .Select(q => q.Id)
                .ToList();

            var userAnswers = _context.UserAnswers
                .Where(a => a.UserId == user.Id && a.ModuleId == moduleId && moduleQuestionIds.Contains(a.QuestionId))
                .OrderByDescending(a => a.Id)
                .Take(totalQuestions)
                .ToList();

            var questionIds = userAnswers.Select(a => a.QuestionId).Distinct().ToList();
            var questions = _context.Questions
                .Where(q => questionIds.Contains(q.Id))
                .ToList();

            foreach (var ans in userAnswers)
                ans.Question = questions.FirstOrDefault(q => q.Id == ans.QuestionId);

            ViewBag.Session = session;
            ViewBag.Profile = profile;
            ViewBag.SessionId = sessionId;
            ViewBag.ModuleId = moduleId;
            ViewBag.TotalCorrect = totalCorrect;
            ViewBag.TotalWrong = totalWrong;
            ViewBag.TotalQuestions = totalQuestions;
            ViewBag.UserAnswers = userAnswers;
            ViewBag.Result = result;

            return View();
        }

        public IActionResult ChangePassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
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