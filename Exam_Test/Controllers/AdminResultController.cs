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

        public IActionResult Index(int? moduleId, string? userId, int page = 1)
        {
            int pageSize = 10;
            var results = _context.Results.AsQueryable();

            if (moduleId != null)
                results = results.Where(r => r.ModuleId == moduleId);

            if (!string.IsNullOrEmpty(userId))
                results = results.Where(r => r.UserId != null && r.UserId.Contains(userId));

            int totalCount = results.Count();

            var data = results
                .OrderByDescending(r => r.ExamDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.ModuleId = moduleId;
            ViewBag.UserId = userId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            return View(data);
        }

        public IActionResult UserResults(string userId)
        {
            var results = _context.Results
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ExamDate)
                .ToList();

            return View(results);
        }

        // NEW: Delete a result
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var result = _context.Results.Find(id);
            if (result != null)
            {
                _context.Results.Remove(result);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}