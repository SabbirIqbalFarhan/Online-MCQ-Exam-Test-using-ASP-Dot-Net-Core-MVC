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
        // START EXAM - CHECK PERMISSION
        // =========================
        public async Task<IActionResult> Start(int moduleId)
        {
            var user = await _userManager.GetUserAsync(User);

            var permission = _context.ExamPermissions
                .FirstOrDefault(p => p.UserId == user.Id && p.IsPermitted == true);

            if (permission == null)
            {
                TempData["Error"] = "You do not have permission to take the exam. Please request access from your dashboard.";
                return RedirectToAction("Dashboard", "User");
            }

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

            // Double check permission on submit too
            var permission = _context.ExamPermissions
                .FirstOrDefault(p => p.UserId == user.Id && p.IsPermitted == true);

            if (permission == null)
            {
                return RedirectToAction("Dashboard", "User");
            }

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
                    Question = q,
                    SelectedAnswer = selected,
                    IsCorrect = isCorrect,
                    ModuleId = moduleId
                });
            }

            _context.UserAnswers.AddRange(userAnswers);

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

        // =========================
        // REVIEW ANSWERS
        // =========================
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