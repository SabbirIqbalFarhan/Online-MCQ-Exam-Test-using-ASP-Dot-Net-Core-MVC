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

        public IActionResult Index()
        {
            var sessions = _context.ExamSessions
                .OrderByDescending(s => s.CreatedAt)
                .ToList();
            return View(sessions);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(string title, DateTime startTime, DateTime endTime)
        {
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

        public IActionResult Activate(int id)
        {
            var all = _context.ExamSessions.ToList();
            foreach (var s in all) s.IsActive = false;

            var session = _context.ExamSessions.Find(id);
            if (session != null) session.IsActive = true;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Deactivate(int id)
        {
            var session = _context.ExamSessions.Find(id);
            if (session != null) session.IsActive = false;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var session = _context.ExamSessions.Find(id);
            if (session != null) _context.ExamSessions.Remove(session);

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Results(int id, int moduleId = 1)
        {
            var session = _context.ExamSessions.Find(id);
            if (session == null) return NotFound();

            var results = _context.Results
                .Where(r => r.SessionId == id && r.ModuleId == moduleId)
                .OrderByDescending(r => r.ExamDate)
                .ToList();

            var profiles = _context.UserProfiles.ToList();

            ViewBag.Session = session;
            ViewBag.ModuleId = moduleId;
            ViewBag.SessionId = id;
            ViewBag.Profiles = profiles;

            return View(results);
        }
    }
}