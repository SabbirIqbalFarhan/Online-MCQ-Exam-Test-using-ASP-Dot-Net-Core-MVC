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

            var session = _context.ExamSessions
    .Where(s => s.IsActive)
    .OrderByDescending(s => s.StartTime)
    .FirstOrDefault()
    ?? _context.ExamSessions
    .Where(s => s.StartTime <= DateTime.Now && s.EndTime >= DateTime.Now)
    .OrderByDescending(s => s.StartTime)
    .FirstOrDefault();

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

            // Prevent re-taking the same module in the same session
            bool alreadyAttempted = _context.Results
                .Any(r => r.UserId == user.Id && r.ModuleId == moduleId && r.SessionId == session.Id);

            if (alreadyAttempted)
            {
                TempData["Error"] = $"You have already completed Module {moduleId} in this session.";
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

            // Prevent double-submit
            bool alreadySubmitted = _context.Results
                .Any(r => r.UserId == user.Id && r.ModuleId == moduleId && r.SessionId == sessionId);

            if (alreadySubmitted)
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

            // Always redirect to result page after submitting
            return RedirectToAction("Result", new { moduleId = moduleId, sessionId = sessionId });
        }

        public async Task<IActionResult> Result(int moduleId, int sessionId)
        {
            var user = await _userManager.GetUserAsync(User);

            var result = _context.Results
                .FirstOrDefault(r => r.UserId == user.Id && r.ModuleId == moduleId && r.SessionId == sessionId);

            if (result == null)
                return RedirectToAction("Dashboard", "User");

            var moduleQuestionIds = _context.Questions
                .Where(q => q.ModuleId == moduleId)
                .Select(q => q.Id)
                .ToList();

            int totalQ = result.Correct + result.Wrong;

            var userAnswers = _context.UserAnswers
                .Where(a => a.UserId == user.Id && a.ModuleId == moduleId && moduleQuestionIds.Contains(a.QuestionId))
                .OrderByDescending(a => a.Id)
                .Take(totalQ)
                .ToList();

            var questionIds = userAnswers.Select(a => a.QuestionId).Distinct().ToList();
            var questions = _context.Questions.Where(q => questionIds.Contains(q.Id)).ToList();

            foreach (var ans in userAnswers)
                ans.Question = questions.FirstOrDefault(q => q.Id == ans.QuestionId);

            ViewBag.ModuleId = moduleId;
            ViewBag.SessionId = sessionId;
            ViewBag.Correct = result.Correct;
            ViewBag.Wrong = result.Wrong;
            ViewBag.UserAnswers = userAnswers;

            return View();
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