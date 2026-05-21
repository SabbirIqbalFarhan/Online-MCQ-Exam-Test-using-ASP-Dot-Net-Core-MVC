using Exam_Test.Data;
using Exam_Test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Test.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSessionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminSessionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 10;
            var allSessions = _context.ExamSessions
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            int total = allSessions.Count;
            var sessions = allSessions.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

            return View(sessions);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string title, DateTime startTime, DateTime endTime)
        {
            bool overlap = _context.ExamSessions.Any(s =>
                startTime < s.EndTime && endTime > s.StartTime
            );

            if (overlap)
            {
                TempData["Error"] = "A session already exists that overlaps with this time range. Please choose a different time.";
                return RedirectToAction("Create");
            }

            _context.ExamSessions.Add(new ExamSession
            {
                Title = title,
                StartTime = startTime,
                EndTime = endTime,
                IsActive = false,
                CreatedAt = DateTime.Now
            });
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Activate(int id)
        {
            var all = _context.ExamSessions.ToList();
            foreach (var s in all) s.IsActive = false;

            var session = _context.ExamSessions.Find(id);
            if (session != null) session.IsActive = true;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deactivate(int id)
        {
            var session = _context.ExamSessions.Find(id);
            if (session != null) session.IsActive = false;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var session = _context.ExamSessions.Find(id);
            if (session == null) return NotFound();

            var results = _context.Results.Where(r => r.SessionId == id).ToList();

            foreach (var res in results)
            {
                var answers = _context.UserAnswers
                    .Where(a => a.UserId == res.UserId && a.ModuleId == res.ModuleId)
                    .ToList();

                _context.UserAnswers.RemoveRange(answers);
            }

            _context.Results.RemoveRange(results);
            _context.ExamSessions.Remove(session);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Results(int id, int? moduleId, string? userId, int page = 1)
        {
            var session = _context.ExamSessions.Find(id);
            if (session == null) return NotFound();

            int pageSize = 10;
            var results = _context.Results.Where(r => r.SessionId == id).AsQueryable();

            if (moduleId.HasValue)
                results = results.Where(r => r.ModuleId == moduleId.Value);

            if (!string.IsNullOrEmpty(userId))
                results = results.Where(r => r.UserId != null && r.UserId.Contains(userId));

            int total = results.Count();

            var data = results
                .OrderByDescending(r => r.ExamDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var profiles = _context.UserProfiles.ToList();

            ViewBag.Session = session;
            ViewBag.ModuleId = moduleId;
            ViewBag.UserId = userId;
            ViewBag.SessionId = id;
            ViewBag.Profiles = profiles;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.TotalCount = total;

            return View(data);
        }

        public IActionResult ViewResult(string userId, int sessionId, int moduleId)
        {
            var session = _context.ExamSessions.Find(sessionId);
            if (session == null) return NotFound();

            var result = _context.Results
                .FirstOrDefault(r => r.UserId == userId && r.SessionId == sessionId && r.ModuleId == moduleId);

            var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == userId);

            int totalCorrect = result?.Correct ?? 0;
            int totalWrong = result?.Wrong ?? 0;
            int totalQuestions = totalCorrect + totalWrong;

            var moduleQuestionIds = _context.Questions
                .Where(q => q.ModuleId == moduleId)
                .Select(q => q.Id)
                .ToList();

            var userAnswers = _context.UserAnswers
                .Where(a => a.UserId == userId && a.ModuleId == moduleId && moduleQuestionIds.Contains(a.QuestionId))
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
            ViewBag.UserId = userId;
            ViewBag.SessionId = sessionId;
            ViewBag.ModuleId = moduleId;
            ViewBag.TotalCorrect = totalCorrect;
            ViewBag.TotalWrong = totalWrong;
            ViewBag.TotalQuestions = totalQuestions;
            ViewBag.UserAnswers = userAnswers;
            ViewBag.Result = result;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteResult(int id, int sessionId)
        {
            var result = _context.Results.Find(id);
            if (result != null) _context.Results.Remove(result);
            _context.SaveChanges();
            return RedirectToAction("Results", new { id = sessionId });
        }
    }
}