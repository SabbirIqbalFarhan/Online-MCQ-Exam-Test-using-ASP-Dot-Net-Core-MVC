using Exam_Test.Data;
using Exam_Test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // =========================
        // ADMIN DASHBOARD
        // =========================
        public IActionResult Dashboard()
        {
            return View();
        }

        // =========================
        // LIST QUESTIONS
        // =========================
        public IActionResult Questions(int moduleId = 1)
        {
            var questions = _context.Questions
                .Where(q => q.ModuleId == moduleId)
                .ToList();

            ViewBag.ModuleId = moduleId;
            return View(questions);
        }

        // =========================
        // ADD QUESTION (GET)
        // =========================
        public IActionResult AddQuestion(int moduleId)
        {
            ViewBag.ModuleId = moduleId;
            return View();
        }

        // =========================
        // ADD QUESTION (POST)
        // =========================
        [HttpPost]
        public async Task<IActionResult> AddQuestion(Question model, IFormFile? imageFile)
        {
            if (imageFile != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "images");
                string fileName = Guid.NewGuid().ToString() + Path.GetFileName(imageFile.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                model.ImagePath = fileName;
            }

            _context.Questions.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Questions", new { moduleId = model.ModuleId });
        }

        // =========================
        // EDIT QUESTION (GET)
        // =========================
        public IActionResult EditQuestion(int id)
        {
            var question = _context.Questions.Find(id);
            return View(question);
        }

        // =========================
        // EDIT QUESTION (POST)
        // =========================
        [HttpPost]
        public async Task<IActionResult> EditQuestion(Question model, IFormFile? imageFile)
        {
            var question = _context.Questions.Find(model.Id);

            if (question == null) return NotFound();

            question.QuestionText = model.QuestionText;
            question.OptionA = model.OptionA;
            question.OptionB = model.OptionB;
            question.OptionC = model.OptionC;
            question.OptionD = model.OptionD;
            question.CorrectAnswer = model.CorrectAnswer;
            question.ModuleId = model.ModuleId;

            if (imageFile != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "images");
                string fileName = Guid.NewGuid().ToString() + Path.GetFileName(imageFile.FileName);
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

        // =========================
        // DELETE QUESTION
        // =========================
        public IActionResult DeleteQuestion(int id)
        {
            var question = _context.Questions.Find(id);

            if (question != null)
            {
                _context.Questions.Remove(question);
                _context.SaveChanges();
            }

            return RedirectToAction("Questions", new { moduleId = question.ModuleId });
        }
    }
}