using Exam_Test.Data;
using Exam_Test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Test.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Dashboard()
        {
            ViewBag.TotalQuestions = _context.Questions.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.PendingRequests = _context.ExamRequests.Count(r => r.Status == "Pending");

            ViewBag.TotalSessions = _context.ExamSessions.Count();

            ViewBag.Profiles = _context.UserProfiles.ToList();

            ViewBag.RecentResults = _context.Results
                .OrderByDescending(r => r.ExamDate)
                .Take(5)
                .ToList();

            return View();
        }

        public IActionResult Questions(int moduleId = 1)
        {
            var questions = _context.Questions
                .Where(q => q.ModuleId == moduleId)
                .ToList();

            ViewBag.ModuleId = moduleId;
            ViewBag.QuestionCount = questions.Count;
            ViewBag.CanAdd = questions.Count < 30;
            return View(questions);
        }

        [HttpGet]
        public IActionResult AddQuestion(int moduleId)
        {
            ViewBag.ModuleId = moduleId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestion(int ModuleId, string? QuestionText, string? OptionA, string? OptionB, string? OptionC, string? CorrectAnswer, IFormFile? imageFile)
        {
            var existingCount = _context.Questions.Count(q => q.ModuleId == ModuleId);

            if (existingCount >= 30)
                return RedirectToAction("Questions", new { moduleId = ModuleId });

            var model = new Question
            {
                ModuleId = ModuleId,
                QuestionText = QuestionText,
                OptionA = OptionA,
                OptionB = OptionB,
                OptionC = OptionC,
                CorrectAnswer = CorrectAnswer
            };

            if (imageFile != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "images");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                model.ImagePath = fileName;
            }

            _context.Questions.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Questions", new { moduleId = ModuleId });
        }

        [HttpGet]
        public IActionResult EditQuestion(int id)
        {
            var question = _context.Questions.Find(id);
            if (question == null) return NotFound();
            return View(question);
        }

        [HttpPost]
        public async Task<IActionResult> EditQuestion(Question model, IFormFile? imageFile)
        {
            var question = _context.Questions.Find(model.Id);

            if (question == null) return NotFound();

            question.QuestionText = model.QuestionText;
            question.OptionA = model.OptionA;
            question.OptionB = model.OptionB;
            question.OptionC = model.OptionC;
            question.CorrectAnswer = model.CorrectAnswer;
            question.ModuleId = model.ModuleId;

            if (imageFile != null)
            {
                if (!string.IsNullOrEmpty(question.ImagePath))
                {
                    string oldPath = Path.Combine(_env.WebRootPath, "images", question.ImagePath);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                string folder = Path.Combine(_env.WebRootPath, "images");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                question.ImagePath = fileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Questions", new { moduleId = model.ModuleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteQuestion(int id)
        {
            var question = _context.Questions.Find(id);

            if (question == null) return NotFound();

            if (!string.IsNullOrEmpty(question.ImagePath))
            {
                string oldPath = Path.Combine(_env.WebRootPath, "images", question.ImagePath);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            int moduleId = question.ModuleId;

            _context.Questions.Remove(question);
            _context.SaveChanges();

            return RedirectToAction("Questions", new { moduleId = moduleId });
        }

    }
}