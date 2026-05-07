using Exam_Test.Data;
using Exam_Test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exam_Test.Controllers
{
    [Authorize(Roles = "User")]
    public class ExamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ExamController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================
        // START EXAM
        // =========================
        public IActionResult Start(int moduleId)
        {
            var questions = _context.Questions
                .Where(q => q.ModuleId == moduleId)
                .Take(30)
                .ToList();

            ViewBag.ModuleId = moduleId;

            return View(questions);
        }

        // =========================
        // SUBMIT EXAM
        // =========================
        [HttpPost]
        public async Task<IActionResult> Submit(int moduleId, Dictionary<int, string> answers)
        {
            var user = await _userManager.GetUserAsync(User);

            var questions = await _context.Questions
                .Where(q => q.ModuleId == moduleId)
                .ToListAsync();

            int correct = 0;
            int wrong = 0;

            List<UserAnswer> userAnswers = new List<UserAnswer>();

            foreach (var q in questions)
            {
                answers.TryGetValue(q.Id, out string selected);

                bool isCorrect = selected == q.CorrectAnswer;

                if (isCorrect) correct++;
                else wrong++;

                userAnswers.Add(new UserAnswer
                {
                    UserId = user.Id,
                    QuestionId = q.Id,
                    Question = q, // 🔥 ADD THIS
                    SelectedAnswer = selected,
                    IsCorrect = isCorrect,
                    ModuleId = moduleId
                });
            }

            // Save answers
            _context.UserAnswers.AddRange(userAnswers);

            // Save result
            var result = new Result
            {
                UserId = user.Id,
                ModuleId = moduleId,
                Correct = correct,
                Wrong = wrong,
                ExamDate = DateTime.Now
            };

            _context.Results.Add(result);

            await _context.SaveChangesAsync();

            ViewBag.Correct = correct;
            ViewBag.Wrong = wrong;
            ViewBag.ModuleId = moduleId;

            return View("Result");
        }

        public async Task<IActionResult> Review(int moduleId, bool onlyWrong = false)
        {
            var user = await _userManager.GetUserAsync(User);

            var query = _context.UserAnswers
                .Include(a => a.Question)
                .Where(a => a.UserId == user.Id && a.ModuleId == moduleId);

            if (onlyWrong)
            {
                query = query.Where(a => !a.IsCorrect);
            }

            var answers = await query.ToListAsync();

            return View(answers);
        }

        // =========================
        // RESULT HISTORY
        // =========================
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);

            var results = await _context.Results
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.ExamDate)
                .ToListAsync();

            return View(results);
        }
    }
}