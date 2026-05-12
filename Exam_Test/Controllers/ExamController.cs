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

        public async Task<IActionResult> Start(int moduleId)
        {
            var user = await _userManager.GetUserAsync(User);

            var permission = _context.ExamPermissions
                .FirstOrDefault(p => p.UserId == user.Id && p.IsPermitted == true);

            if (permission == null)
            {
                TempData["Error"] = "You do not have permission to take the exam.";
                return RedirectToAction("Dashboard", "User");
            }

            var session = _context.ExamSessions.FirstOrDefault(s => s.IsActive);

            if (session == null)
            {
                TempData["Error"] = "No active exam session at the moment. Please wait for the admin to start a session.";
                return RedirectToAction("Dashboard", "User");
            }

            if (DateTime.Now < session.StartTime)
            {
                TempData["Error"] = $"The exam session has not started yet. It will begin at {session.StartTime:dd MMM yyyy, hh:mm tt}.";
                return RedirectToAction("Dashboard", "User");
            }

            if (DateTime.Now > session.EndTime)
            {
                TempData["Error"] = "The exam session has ended.";
                return RedirectToAction("Dashboard", "User");
            }

            var questions = _context.Questions
                .Where(q => q.ModuleId == moduleId)
                .ToList();

            ViewBag.ModuleId = moduleId;
            ViewBag.SessionId = session.Id;
            ViewBag.SessionTitle = session.Title;

            return View(questions);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int moduleId, int sessionId, Dictionary<int, string> answers)
        {
            var user = await _userManager.GetUserAsync(User);

            var permission = _context.ExamPermissions
                .FirstOrDefault(p => p.UserId == user.Id && p.IsPermitted == true);

            if (permission == null)
                return RedirectToAction("Dashboard", "User");

            var questions = await _context.Questions
                .Where(q => q.ModuleId == moduleId)
                .ToListAsync();

            int correct = 0, wrong = 0;
            var userAnswers = new List<UserAnswer>();

            foreach (var q in questions)
            {
                answers.TryGetValue(q.Id, out string selected);
                bool isCorrect = selected == q.CorrectAnswer;
                if (isCorrect) correct++; else wrong++;

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
            _context.Results.Add(new Result
            {
                UserId = user.Id,
                ModuleId = moduleId,
                Correct = correct,
                Wrong = wrong,
                ExamDate = DateTime.Now,
                SessionId = sessionId
            });

            await _context.SaveChangesAsync();

            int nextModule = moduleId + 1;

            if (nextModule <= 3)
            {
                TempData["ModuleResult"] = $"✅ Module {moduleId} completed! Correct: {correct}, Wrong: {wrong}. Now starting Module {nextModule}...";
                return RedirectToAction("Start", new { moduleId = nextModule });
            }

            TempData["ExamDone"] = $"true";
            TempData["FinalCorrect"] = correct.ToString();
            TempData["FinalWrong"] = wrong.ToString();
            return RedirectToAction("Dashboard", "User");
        }

        public async Task<IActionResult> Review(int moduleId, bool onlyWrong = false)
        {
            var user = await _userManager.GetUserAsync(User);

            var query = _context.UserAnswers
                .Include(a => a.Question)
                .Where(a => a.UserId == user.Id && a.ModuleId == moduleId);

            if (onlyWrong) query = query.Where(a => !a.IsCorrect);

            var answers = await query.ToListAsync();
            return View(answers);
        }

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