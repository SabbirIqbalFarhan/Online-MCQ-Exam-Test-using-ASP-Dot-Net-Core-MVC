using Exam_Test.Data;
using Exam_Test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;

namespace Exam_Test.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly HttpClient _httpClient;
        private const string ImgBBApiKey = "cb3c05b445dff441a5e520dfb59d421e";

        public AdminController(ApplicationDbContext context, IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _env = env;
            _httpClient = httpClientFactory.CreateClient();
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(int ModuleId, string? QuestionText, string? OptionA, string? OptionB, string? OptionC, string? CorrectAnswer, IFormFile? imageFile)
        {
            var existingCount = _context.Questions.Count(q => q.ModuleId == ModuleId);
            if (existingCount >= 30)
                return RedirectToAction("Questions", new { moduleId = ModuleId });

            string? imagePath = null;
            string? imgBBDeleteUrl = null;

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadResult = await UploadToImgBB(imageFile);
                imagePath = uploadResult.ImageUrl;
                imgBBDeleteUrl = uploadResult.DeleteUrl;
            }

            var model = new Question
            {
                ModuleId = ModuleId,
                QuestionText = QuestionText,
                OptionA = OptionA,
                OptionB = OptionB,
                OptionC = OptionC,
                CorrectAnswer = CorrectAnswer,
                ImagePath = imagePath,
                ImgBBDeleteUrl = imgBBDeleteUrl,
                ImageData = null
            };

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
        [ValidateAntiForgeryToken]
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
            question.ImageData = null;

            bool removeImage = Request.Form["removeImage"] == "true";

            if (removeImage)
            {
                await DeleteFromImgBB(question.ImgBBDeleteUrl);
                question.ImagePath = null;
                question.ImgBBDeleteUrl = null;
            }
            else if (imageFile != null && imageFile.Length > 0)
            {
                // Delete old image first
                await DeleteFromImgBB(question.ImgBBDeleteUrl);

                // Upload new image
                var uploadResult = await UploadToImgBB(imageFile);
                question.ImagePath = uploadResult.ImageUrl;
                question.ImgBBDeleteUrl = uploadResult.DeleteUrl;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Questions", new { moduleId = model.ModuleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = _context.Questions.Find(id);
            if (question == null) return NotFound();

            // Delete image from ImgBB
            await DeleteFromImgBB(question.ImgBBDeleteUrl);

            int moduleId = question.ModuleId;

            var relatedAnswers = _context.UserAnswers.Where(a => a.QuestionId == id).ToList();
            _context.UserAnswers.RemoveRange(relatedAnswers);

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return RedirectToAction("Questions", new { moduleId = moduleId });
        }

        // ---------- ImgBB Helper Methods ----------

        private async Task<(string? ImageUrl, string? DeleteUrl)> UploadToImgBB(IFormFile imageFile)
        {
            using var ms = new MemoryStream();
            await imageFile.CopyToAsync(ms);
            var base64Image = Convert.ToBase64String(ms.ToArray());

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(ImgBBApiKey), "key");
            formData.Add(new StringContent(base64Image), "image");

            var response = await _httpClient.PostAsync("https://api.imgbb.com/1/upload", formData);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");
            var imageUrl = data.GetProperty("url").GetString();
            var deleteUrl = data.GetProperty("delete_url").GetString();

            return (imageUrl, deleteUrl);
        }

        private async Task DeleteFromImgBB(string? deleteUrl)
        {
            if (string.IsNullOrEmpty(deleteUrl)) return;
            try
            {
                await _httpClient.GetAsync(deleteUrl);
            }
            catch { }
        }
    }
}