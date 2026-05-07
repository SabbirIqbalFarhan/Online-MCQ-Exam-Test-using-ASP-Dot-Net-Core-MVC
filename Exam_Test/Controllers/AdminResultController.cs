using Exam_Test.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exam_Test.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminResultController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminResultController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // ALL RESULTS
        // =========================
        public IActionResult Index(int? moduleId)
        {
            var results = _context.Results.AsQueryable();

            if (moduleId != null)
            {
                results = results.Where(r => r.ModuleId == moduleId);
            }

            var data = results
                .OrderByDescending(r => r.ExamDate)
                .ToList();

            ViewBag.ModuleId = moduleId;

            return View(data);
        }

        // =========================
        // USER DETAIL RESULT
        // =========================
        public IActionResult UserResults(string userId)
        {
            var results = _context.Results
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ExamDate)
                .ToList();

            return View(results);
        }
    }
}